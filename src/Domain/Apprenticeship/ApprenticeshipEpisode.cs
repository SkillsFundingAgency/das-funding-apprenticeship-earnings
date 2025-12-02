using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Learning.Types;
using System.Collections.ObjectModel;
using BreaksInLearningCalculator = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations.BreaksInLearning;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode : AggregateComponent
{

    private ApprenticeshipEpisode(EpisodeModel model, DateTime dateOfBirth, Action<AggregateComponent> addChildToRoot) : base(addChildToRoot)
    {
        _model = model;

        _prices = _model.Prices.Select(Price.Get).ToList();
        _breaksInLearning = _model.BreaksInLearning.Select(EpisodeBreakInLearning.Get).ToList();
        if (_model.EarningsProfile != null)
        {
            _earningsProfile = this.GetEarningsProfileFromModel(_model.EarningsProfile);
        }

        UpdateAgeAtStart(dateOfBirth);
    }

    internal static ApprenticeshipEpisode Get(Apprenticeship apprenticeship, EpisodeModel entity)
    {
        var episode = new ApprenticeshipEpisode(entity, apprenticeship.DateOfBirth, apprenticeship.AddChildToRoot);
        return episode;
    }

    private readonly EpisodeModel _model;
    private List<Price> _prices;
    private List<EpisodeBreakInLearning> _breaksInLearning;
    private EarningsProfile? _earningsProfile;
    private int _ageAtStartOfApprenticeship;

    public Guid ApprenticeshipEpisodeKey => _model.Key;
    public long UKPRN => _model.Ukprn;
    public long EmployerAccountId => _model.EmployerAccountId;
    public int AgeAtStartOfApprenticeship => _ageAtStartOfApprenticeship;
    public string TrainingCode => _model.TrainingCode;
    public FundingType FundingType => _model.FundingType;
    public string LegalEntityName => _model.LegalEntityName;
    public long? FundingEmployerAccountId => _model.FundingEmployerAccountId;
    public EarningsProfile? EarningsProfile => _earningsProfile;
    public IReadOnlyCollection<Price> Prices => new ReadOnlyCollection<Price>(_prices);
    public IReadOnlyCollection<EpisodeBreakInLearning> BreaksInLearning => new ReadOnlyCollection<EpisodeBreakInLearning>(_breaksInLearning);
    public bool IsNonLevyFullyFunded => _model.FundingType == FundingType.NonLevy && _ageAtStartOfApprenticeship < 22;
    public DateTime? CompletionDate => _model.CompletionDate;
    public DateTime? WithdrawalDate => _model.WithdrawalDate;
    public DateTime? PauseDate => _model.PauseDate;
    public decimal FundingBandMaximum => _model.FundingBandMaximum;
    public DateTime? LastDayOfLearning => this.GetLastDayOfLearning();

    public string FundingLineType =>
        _ageAtStartOfApprenticeship < 19
            ? "16-18 Apprenticeship (Employer on App Service)"
            : "19+ Apprenticeship (Employer on App Service)";


    public void CalculateOnProgram(Apprenticeship apprenticeship, ISystemClockService systemClock)
    {
        var(instalments, additionalPayments, onProgramTotal, completionPayment) = GenerateBasicEarnings(apprenticeship);

        if (BreaksInLearning.Any())
        {
            instalments = BreaksInLearningCalculator.RecalculateInstalments(instalments, BreaksInLearning);
        }

        if (_model.CompletionDate != null)
        {
            instalments = BalancingInstalments.BalanceInstalmentsForCompletion(_model.CompletionDate.Value, instalments, _model.Prices.Max(x => x.EndDate));
            var completionInstalment = CompletionInstalments.GenerationCompletionInstalment(_model.CompletionDate.Value, completionPayment, instalments.MaxBy(x => x.AcademicYear + x.DeliveryPeriod)!.EpisodePriceKey);
            instalments = instalments.Append(completionInstalment).ToList();
        }

        if(LastDayOfLearning.HasValue)
        {
            instalments = OnProgramPayments.SoftDeleteAfterLastDayOfLearning(instalments, _prices, LastDayOfLearning.Value);
            additionalPayments = AdditionalPayments.SoftDeleteAfterLastDayOfLearning(additionalPayments, LastDayOfLearning.Value);
        }

        if (_earningsProfile == null)
        {
            _earningsProfile = this.CreateEarningsProfile(onProgramTotal, instalments, additionalPayments, new List<MathsAndEnglish>(), completionPayment, ApprenticeshipEpisodeKey);
            _model.EarningsProfile = _earningsProfile.GetModel();
        }
        else
        {
            additionalPayments.AddRange(EarningsProfile!.PersistentAdditionalPayments());
            _earningsProfile.Update(systemClock,
                instalments: instalments,
                additionalPayments: additionalPayments,
                onProgramTotal: onProgramTotal,
                completionPayment: completionPayment);
        }
    }

    public void Withdraw(DateTime? withdrawalDate, ISystemClockService systemClock)
    {
        _model.WithdrawalDate = withdrawalDate;
    }

    public void ReverseWithdrawal(ISystemClockService systemClockService)
    {
        _model.WithdrawalDate = null;
    }

    public void WithdrawMathsAndEnglish(string courseName, DateTime? withdrawalDate, ISystemClockService systemClock)
    {
        var courseToWithdraw = _model.EarningsProfile.MathsAndEnglishCourses.SingleOrDefault(x => x.Course == courseName);
        if (courseToWithdraw == null) throw new ArgumentException($"No english and maths course found for course name {courseName}", nameof(courseName));

        courseToWithdraw.WithdrawalDate = withdrawalDate;
        var updatedCourses = ReEvaluateMathsAndEnglishEarningsAfterEndOfCourse(systemClock, _model.EarningsProfile.MathsAndEnglishCourses, courseName);
        _earningsProfile.Update(systemClock, mathsAndEnglishCourses: updatedCourses);
    }

    public List<MathsAndEnglish> ReEvaluateMathsAndEnglishEarningsAfterEndOfCourse(ISystemClockService systemClock, List<MathsAndEnglishModel> courses, string? courseName = null)
    {
        MathsAndEnglishModel? course;

        if (courseName != null)
        {
            course = courses.SingleOrDefault(x => x.Course == courseName);
            if (course == null) throw new ArgumentException($"No english and maths course found for course name {courseName}", courseName);
        }

        var updatedCourses = new List<MathsAndEnglish>();

        foreach (var mathsAndEnglishModel in courses)
        {
            var skipReevaluation = courseName != null && mathsAndEnglishModel.Course != courseName;
            if (skipReevaluation)
            {
                updatedCourses.Add(new MathsAndEnglish(
                    mathsAndEnglishModel.StartDate,
                    mathsAndEnglishModel.EndDate,
                    mathsAndEnglishModel.Course,
                    mathsAndEnglishModel.Amount,
                    mathsAndEnglishModel.Instalments.Select(x => new MathsAndEnglishInstalment(x.AcademicYear,
                        x.DeliveryPeriod, x.Amount, Enum.Parse<MathsAndEnglishInstalmentType>(x.Type), x.IsAfterLearningEnded)).ToList(),
                    mathsAndEnglishModel.WithdrawalDate,
                    mathsAndEnglishModel.ActualEndDate,
                    mathsAndEnglishModel.PauseDate,
                    mathsAndEnglishModel.PriorLearningAdjustmentPercentage
                ));
            }
            else
            {
                var lastDayOfLearning = new[] { mathsAndEnglishModel.WithdrawalDate, mathsAndEnglishModel.PauseDate }.Where(d => d.HasValue).OrderBy(d => d.Value).FirstOrDefault();
                var earningsToKeep = GetMathsAndEnglishEarningsToKeep(mathsAndEnglishModel, lastDayOfLearning);

                updatedCourses.Add(new MathsAndEnglish(
                    mathsAndEnglishModel.StartDate,
                    mathsAndEnglishModel.EndDate,
                    mathsAndEnglishModel.Course,
                    mathsAndEnglishModel.Amount,
                    mathsAndEnglishModel.Instalments.Select(x => new MathsAndEnglishInstalment(x.AcademicYear,
                        x.DeliveryPeriod, x.Amount, Enum.Parse<MathsAndEnglishInstalmentType>(x.Type), !earningsToKeep.Any(e => 
                            e.AcademicYear == x.AcademicYear && 
                            e.DeliveryPeriod == x.DeliveryPeriod &&
                            e.Amount == x.Amount &&
                            e.Type == x.Type))).ToList(),
                    mathsAndEnglishModel.WithdrawalDate,
                    mathsAndEnglishModel.ActualEndDate,
                    mathsAndEnglishModel.PauseDate,
                    mathsAndEnglishModel.PriorLearningAdjustmentPercentage
                ));
            }
        }

        return updatedCourses;
    }

    /// <summary>
    /// Adds additional earnings to an apprenticeship that are not included in the standard earnings calculation process.
    /// Some earnings are generated separately using this endpoint, while others are handled as part of the normal process.
    /// </summary>
    public void AddAdditionalEarnings(List<AdditionalPayment> additionalPayments, string additionalPaymentType, ISystemClockService systemClock)
    {
        // verify that all additional payments are of the same type
        if (additionalPayments.Select(x => x.AdditionalPaymentType).Distinct().Count() > 1)
        {
            throw new InvalidOperationException("All additional payments must be of the same type.");
        }

        // Retain only additional payments of a different type
        var existingAdditionalPayments = EarningsProfile!.AdditionalPayments.Where(x => x.AdditionalPaymentType != additionalPaymentType).ToList();

        existingAdditionalPayments.AddRange(additionalPayments);

        _earningsProfile!.Update(
            systemClock,
            additionalPayments: existingAdditionalPayments);
    }

    /// <summary>
    /// Updates earnings for Maths and English courses to an apprenticeship.
    /// Overwrites any existing Maths and English courses' earnings.
    /// </summary>
    public void UpdateMathsAndEnglishCourses(List<MathsAndEnglish> mathsAndEnglishCourses, ISystemClockService systemClock)
    {
        var updatedCourses = ReEvaluateMathsAndEnglishEarningsAfterEndOfCourse(systemClock, mathsAndEnglishCourses.Select(x => x.GetModel()).ToList());
        _earningsProfile!.Update(systemClock, mathsAndEnglishCourses: updatedCourses);
    }

    /// <summary>
    /// Updates the completion date and earnings profile accordingly with the completion instalment and balanced instalments if necessary.
    /// </summary>
    public void UpdateCompletion(Apprenticeship apprenticeship, DateTime? completionDate, ISystemClockService systemClock)
    {
        _model.CompletionDate = completionDate;
    }

    private List<MathsAndEnglishInstalmentModel> GetMathsAndEnglishEarningsToKeep(MathsAndEnglishModel course, DateTime? lastDayOfLearning)
    {
        if (!lastDayOfLearning.HasValue)
        {
            return course.Instalments.ToList();
        }

        var academicYear = lastDayOfLearning.Value.ToAcademicYear();
        var deliveryPeriod = lastDayOfLearning.Value.ToDeliveryPeriod();

        var instalments = course.Instalments
            .Where(x =>
                x.AcademicYear < academicYear //keep earnings from previous academic years
                || x.AcademicYear == academicYear && x.DeliveryPeriod < deliveryPeriod //keep earnings from previous delivery periods in the same academic year
                || x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod 
                                                  && lastDayOfLearning.Value.Day == DateTime.DaysInMonth(lastDayOfLearning.Value.Year, lastDayOfLearning.Value.Month) //keep earnings in the last delivery period of learning if the learner is in learning on the census date
                || x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod 
                                                  && course.StartDate.ToAcademicYear() == academicYear && course.StartDate.ToDeliveryPeriod() == deliveryPeriod 
                                                  && lastDayOfLearning.Value > course.StartDate) // special case if the withdrawal date is on/after the start date but before a census date we should keep the instalment for the first month of learning
            .ToList(); 

        return instalments;
    }

    internal void UpdateFundingBandMaximum(int fundingBandMaximum)
    {
        _model.FundingBandMaximum = fundingBandMaximum;
    }

    internal void UpdatePrices(List<Learning.Types.LearningEpisodePrice> updatedPrices, int ageAtStartOfLearning)
    {
        _ageAtStartOfApprenticeship = ageAtStartOfLearning;

        foreach (var existingPrice in _prices.ToList())
        {
            var updatedPrice = updatedPrices.SingleOrDefault(x => x.Key == existingPrice.PriceKey);
            if (updatedPrice != null)
            {
                existingPrice.Update(updatedPrice.StartDate, updatedPrice.EndDate, updatedPrice.TotalPrice);
            }
            else
            {
                _prices.Remove(existingPrice);
                _model.Prices.Remove(existingPrice.GetModel());
            }
        }

        var newPrices = updatedPrices
            .Where(x => _prices.All(y => y.PriceKey != x.Key))
            .Select(x => new Price(x.Key, x.StartDate, x.EndDate, x.TotalPrice))
            .ToList();
        _model.Prices.AddRange(newPrices.Select(x => x.GetModel()));
        _prices.AddRange(newPrices);
    }

    internal bool PricesAreIdentical(List<LearningEpisodePrice> prices)
    {
        if (prices.Count != _prices.Count)
            return false;

        foreach (var price in prices)
        {
            var matchingPrice = _prices.SingleOrDefault(x => x.PriceKey == price.Key);
            if (matchingPrice == null)
                return false;
            if (matchingPrice.StartDate != price.StartDate || matchingPrice.EndDate != price.EndDate || matchingPrice.AgreedPrice != price.TotalPrice)
                return false;
        }

        return true;
    }

    internal void UpdatePause(DateTime? pauseDate)
    {
        _model.PauseDate = pauseDate;
    }

    public void UpdateBreaksInLearning(List<EpisodeBreakInLearning> newBreaks)
    {
        // Remove breaks that are no longer present
        foreach (var existingBreak in _breaksInLearning.ToList())
        {
            bool stillExists = newBreaks.Any(nb =>
                nb.StartDate == existingBreak.StartDate &&
                nb.EndDate == existingBreak.EndDate);

            if (!stillExists)
            {
                _breaksInLearning.Remove(existingBreak);
                _model.BreaksInLearning.Remove(existingBreak.GetModel());
            }
        }

        // Add new breaks that do not already exist
        foreach (var newBreak in newBreaks)
        {
            bool alreadyExists = _breaksInLearning.Any(eb =>
                eb.StartDate == newBreak.StartDate &&
                eb.EndDate == newBreak.EndDate);

            if (!alreadyExists)
            {
                _breaksInLearning.Add(newBreak);
                _model.BreaksInLearning.Add(newBreak.GetModel());
            }
        }
    }

    // This will generate instalments and additional payments not taking into account
    // any external factors (e.g. a break in learning, these will be applied later)
    private (List<Instalment> instalments, List<AdditionalPayment> additionalPayments, decimal onProgramTotal, decimal completionPayment) GenerateBasicEarnings(Apprenticeship apprenticeship)
    {
        var onProgramPayments = OnProgramPayments.GenerateEarningsForEpisodePrices(Prices, FundingBandMaximum, out var onProgramTotal, out var completionPayment);
        var instalments = onProgramPayments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.PriceKey)).ToList();

        var effectiveEndDate = LastDayOfLearning ?? _prices.Max(p => p.EndDate);

        var incentivePayments = IncentivePayments.GenerateIncentivePayments(
            _ageAtStartOfApprenticeship,
            _prices.Min(p => p.StartDate),
            effectiveEndDate,
            apprenticeship.HasEHCP,
            apprenticeship.IsCareLeaver,
            apprenticeship.CareLeaverEmployerConsentGiven,
            _breaksInLearning);

        var additionalPayments = incentivePayments.Select(x => new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.IncentiveType)).ToList();

        return (instalments, additionalPayments, onProgramTotal, completionPayment);
    }

    internal void UpdateAgeAtStart(DateTime dateOfBirth)
    {
        _ageAtStartOfApprenticeship = dateOfBirth.CalculateAgeAtDate(_prices.Min(x => x.StartDate));
    }
}