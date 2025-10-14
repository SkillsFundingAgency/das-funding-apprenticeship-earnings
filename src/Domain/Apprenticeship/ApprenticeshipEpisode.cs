using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode : AggregateComponent
{

    private ApprenticeshipEpisode(EpisodeModel model, Action<AggregateComponent> addChildToRoot):base(addChildToRoot)
    {
        _model = model;

        _prices = _model.Prices.Select(Price.Get).ToList();
        if (_model.EarningsProfile != null)
        {
            _earningsProfile = this.GetEarningsProfileFromModel(_model.EarningsProfile);
        }
    }

    internal static ApprenticeshipEpisode Get(Apprenticeship apprenticeship, EpisodeModel entity)
    {
        var episode = new ApprenticeshipEpisode(entity, apprenticeship.AddChildToRoot);
        return episode;
    }

    private readonly EpisodeModel _model;
    private List<Price> _prices;
    private EarningsProfile? _earningsProfile;

    public Guid ApprenticeshipEpisodeKey => _model.Key;
    public long UKPRN => _model.Ukprn;
    public long EmployerAccountId => _model.EmployerAccountId;
    public int AgeAtStartOfApprenticeship => _model.AgeAtStartOfApprenticeship;
    public string TrainingCode => _model.TrainingCode;
    public FundingType FundingType => _model.FundingType;
    public string LegalEntityName => _model.LegalEntityName;
    public long? FundingEmployerAccountId => _model.FundingEmployerAccountId;
    public EarningsProfile? EarningsProfile => _earningsProfile;
    public IReadOnlyCollection<Price> Prices => new ReadOnlyCollection<Price>(_prices);
    public bool IsNonLevyFullyFunded => _model.FundingType == FundingType.NonLevy && _model.AgeAtStartOfApprenticeship < 22;
    public DateTime? CompletionDate => _model.CompletionDate;

    public string FundingLineType =>
        AgeAtStartOfApprenticeship < 19
            ? "16-18 Apprenticeship (Employer on App Service)"
            : "19+ Apprenticeship (Employer on App Service)";

    public void CalculateEpisodeEarnings(Apprenticeship apprenticeship, ISystemClockService systemClock)
    {
        var onProgramPayments = OnProgramPayments.GenerateEarningsForEpisodePrices(Prices, out var onProgramTotal, out var completionPayment);
        var instalments = onProgramPayments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.PriceKey)).ToList();

        var incentivePayments = IncentivePayments.GenerateIncentivePayments(
            AgeAtStartOfApprenticeship, 
            _prices.Min(p => p.StartDate), 
            _prices.Max(p => p.EndDate),
            apprenticeship.HasEHCP,
            apprenticeship.IsCareLeaver,
            apprenticeship.CareLeaverEmployerConsentGiven);

        var additionalPayments = incentivePayments.Select(x => new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.IncentiveType)).ToList();
        

        if(_earningsProfile == null)
        {
            _earningsProfile = this.CreateEarningsProfile(onProgramTotal, instalments, additionalPayments, new List<MathsAndEnglish>(), completionPayment, ApprenticeshipEpisodeKey);
            _model.EarningsProfile = _earningsProfile.GetModel();
        }
        else
        {
            additionalPayments.AddRange(EarningsProfile!.PersistentAdditionalPayments());
            _earningsProfile.Update(systemClock, 
                instalments: instalments, 
                additionalPayments:additionalPayments,
                onProgramTotal:onProgramTotal,
                completionPayment:completionPayment);
        }
    }

    public void Withdraw(DateTime? withdrawalDate, ISystemClockService systemClock)
    {
        ReEvaluateEarningsAfterEndOfLearning(withdrawalDate, systemClock);
    }

    public void ReverseWithdrawal(ISystemClockService systemClockService)
    {
        ReEvaluateEarningsAfterEndOfLearning(null, systemClockService);
    }

    public void ReEvaluateEarningsAfterEndOfLearning(DateTime? withdrawalDate, ISystemClockService systemClock)
    {
        var earningsToKeep = GetEarningsToKeep(withdrawalDate);
        var additionalPaymentsToKeep = GetAdditionalPaymentsToKeep(withdrawalDate);

        var updatedInstalments = _model.EarningsProfile.Instalments
            .Select(x => new Instalment(
                x.AcademicYear,
                x.DeliveryPeriod,
                x.Amount,
                x.EpisodePriceKey,
                Enum.Parse<InstalmentType>(x.Type),
                !earningsToKeep.Contains(x)))
            .ToList();

        var updatedAdditionalPayments = _model.EarningsProfile.AdditionalPayments
            .Select(x => new AdditionalPayment(
                x.AcademicYear,
                x.DeliveryPeriod,
                x.Amount,
                x.DueDate,
                x.AdditionalPaymentType,
                !additionalPaymentsToKeep.Contains(x)))
            .ToList();

        _earningsProfile.Update(systemClock, instalments: updatedInstalments, additionalPayments: updatedAdditionalPayments);
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

        _earningsProfile.Update(
            systemClock,
            additionalPayments: existingAdditionalPayments);
    }

    /// <summary>
    /// Updates earnings for Maths and English courses to an apprenticeship.
    /// Overwrites any existing Maths and English courses' earnings.
    /// </summary>
    public void UpdateMathsAndEnglishCourses(List<MathsAndEnglish> mathsAndEnglishCourses, ISystemClockService systemClock)
    {
        _earningsProfile.Update(systemClock,
            mathsAndEnglishCourses: mathsAndEnglishCourses);
    }

    /// <summary>
    /// Updates the completion date and earnings profile accordingly with the completion instalment and balanced instalments if necessary.
    /// </summary>
    public void UpdateCompletion(Apprenticeship apprenticeship, DateTime? completionDate, ISystemClockService systemClock)
    {
        if(!completionDate.HasValue && !_model.CompletionDate.HasValue)
            return; // No change

        // If previously completed, clear existing completion and balancing instalments. And recalculate instalments
        if (_model.CompletionDate != null)
        {
            _model.CompletionDate = null;
            _earningsProfile!.Update(systemClock, completionPayment: 0);
            CalculateEpisodeEarnings(apprenticeship, systemClock);

            if(completionDate.HasValue)
            {
                _earningsProfile.PurgeEventsOfType<EarningsProfileUpdatedEvent>();// The version will be updated and archive event raised later
            }
            else
            {
                return; // No new completion date provided, so just return after recalculating earnings without purging update event
            }
        }

        var existingInstalments = _earningsProfile?.Instalments.Where(x=>x.Type != InstalmentType.Completion && x.Type != InstalmentType.Balancing).ToList() ?? new List<Instalment>();
        var balancedInstalments = BalancingInstalments.BalanceInstalmentsForCompletion(completionDate!.Value, existingInstalments, _model.Prices.Max(x => x.EndDate));
        var completionInstalment = CompletionInstalments.GenerationCompletionInstalment(completionDate!.Value, _earningsProfile!.CompletionPayment, existingInstalments.MaxBy(x => x.AcademicYear + x.DeliveryPeriod)!.EpisodePriceKey);

        _earningsProfile.Update(systemClock,
            instalments: balancedInstalments.Append(completionInstalment).ToList());

        _model.CompletionDate = completionDate;
    }

    private List<AdditionalPaymentModel> GetAdditionalPaymentsToKeep(DateTime? lastDayOfLearning)
    {
        if (!lastDayOfLearning.HasValue)
        {
            return _model.EarningsProfile.AdditionalPayments;
        }

        var academicYear = lastDayOfLearning.Value.ToAcademicYear();
        var deliveryPeriod = lastDayOfLearning.Value.ToDeliveryPeriod();

        var additionalPayments = _model.EarningsProfile.AdditionalPayments
            .Where(x =>
                x.AcademicYear < academicYear //keep earnings from previous academic years
                || x.AcademicYear == academicYear && x.DeliveryPeriod < deliveryPeriod //keep earnings from previous delivery periods in the same academic year
                || x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod && lastDayOfLearning.Value.Day ==
                DateTime.DaysInMonth(lastDayOfLearning.Value.Year, lastDayOfLearning.Value.Month))
            .ToList(); //keep earnings in the last delivery period of learning if the learner is in learning on the census date

        return additionalPayments;
    }

    private List<InstalmentModel> GetEarningsToKeep(DateTime? lastDayOfLearning)
    {
        if (!lastDayOfLearning.HasValue)
        {
            return _model.EarningsProfile.Instalments;
        }

        List<InstalmentModel> result;

        var academicYear = lastDayOfLearning.Value.ToAcademicYear();
        var deliveryPeriod = lastDayOfLearning.Value.ToDeliveryPeriod();

        var startDate = _model.Prices.Min(x => x.StartDate);
        var qualifyingPeriodDays = GetQualifyingPeriodDays(startDate, _model.Prices.Max(x => x.EndDate));
        var qualifyingDate = startDate.AddDays(qualifyingPeriodDays - 1); //With shorter apprenticeships, this qualifying period will change
        if (lastDayOfLearning < qualifyingDate)
        {
            result = _model.EarningsProfile.Instalments.Where(x =>
                    x.AcademicYear < academicYear) //keep earnings from previous academic years
                .ToList();
        }
        else
        {
            result = _model.EarningsProfile.Instalments.Where(x =>
                    x.AcademicYear < academicYear //keep earnings from previous academic years
            || x.AcademicYear == academicYear && x.DeliveryPeriod < deliveryPeriod //keep earnings from previous delivery periods in the same academic year
            || x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod && lastDayOfLearning.Value.Day == DateTime.DaysInMonth(lastDayOfLearning.Value.Year, lastDayOfLearning.Value.Month)) //keep earnings in the last delivery period of learning if the learner is in learning on the census date
                .ToList();
        }

        return result;
    }

    internal void UpdatePrices(List<Learning.Types.LearningEpisodePrice> updatedPrices, int ageAtStartOfLearning)
    {
        _model.AgeAtStartOfApprenticeship = ageAtStartOfLearning;

        foreach (var existingPrice in _prices.ToList())
        {
            var updatedPrice = updatedPrices.SingleOrDefault(x => x.Key == existingPrice.PriceKey);
            if (updatedPrice != null)
            {
                existingPrice.Update(updatedPrice.StartDate, updatedPrice.EndDate, updatedPrice.TotalPrice, updatedPrice.FundingBandMaximum);
            }
            else
            {
                _prices.Remove(existingPrice);
                _model.Prices.Remove(existingPrice.GetModel());
            }
        }

        var newPrices = updatedPrices
            .Where(x => _prices.All(y => y.PriceKey != x.Key))
            .Select(x => new Price(x.Key, x.StartDate, x.EndDate, x.TotalPrice, x.FundingBandMaximum))
            .ToList();
        _model.Prices.AddRange(newPrices.Select(x => x.GetModel()));
        _prices.AddRange(newPrices);
    }

    private int GetQualifyingPeriodDays(DateTime startDate, DateTime plannedEndDate)
    {
        var plannedDuration = (int)Math.Floor((plannedEndDate - startDate).TotalDays) + 1;
        return plannedDuration switch
        {
            >= 168 => 42,
            >= 14 => 14,
            _ => 1
        };
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
            if (matchingPrice.StartDate != price.StartDate || matchingPrice.EndDate != price.EndDate || matchingPrice.AgreedPrice != price.TotalPrice || matchingPrice.FundingBandMaximum != price.FundingBandMaximum)
                return false;
        }

        return true;
    }
}