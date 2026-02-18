using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using System.Collections.ObjectModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode : AggregateComponent
{

    private ApprenticeshipEpisode(EpisodeModel model, DateTime dateOfBirth, Action<AggregateComponent> addChildToRoot) : base(addChildToRoot)
    {
        _model = model;

        _prices = _model.Prices.Select(Price.Get).ToList();
        _periodsInLearning = _model.PeriodsInLearning.Select(EpisodePeriodInLearning.Get).ToList();
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
    private List<EpisodePeriodInLearning> _periodsInLearning;
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
    public IReadOnlyCollection<EpisodePeriodInLearning> EpisodePeriodsInLearning => new ReadOnlyCollection<EpisodePeriodInLearning>(_periodsInLearning);
    public bool IsNonLevyFullyFunded => _model.FundingType == FundingType.NonLevy && AgeAtStartOfApprenticeship < 22;
    public DateTime? CompletionDate => _model.CompletionDate;
    public DateTime? WithdrawalDate => _model.WithdrawalDate;
    public DateTime? PauseDate => _model.PauseDate;
    public decimal FundingBandMaximum => _model.FundingBandMaximum;
    public DateTime? LastDayOfLearning => this.GetLastDayOfLearning();
    public List<(EpisodePeriodInLearning, List<PriceInPeriod>)> PeriodsInLearningWithMatchedPrices => EpisodePeriodsInLearning.Select(x => this.GetPricesForPeriod(x, Prices.ToList())).ToList(); //todo don't need this but some of the linked code (extensions) will be useful

    public string FundingLineType =>
        AgeAtStartOfApprenticeship < 19
            ? "16-18 Apprenticeship (Employer on App Service)"
            : "19+ Apprenticeship (Employer on App Service)";


    public void CalculateOnProgram(Apprenticeship apprenticeship, ISystemClockService systemClock, string calculationData)
    {
        var(instalments, additionalPayments, onProgramTotal, completionPayment) = GenerateBasicEarnings(apprenticeship);

        if (_model.CompletionDate != null)
        {
            instalments = BalancingInstalments.BalanceInstalmentsForCompletion(_model.CompletionDate.Value, instalments, _model.Prices.Max(x => x.EndDate));
            var completionInstalment = CompletionInstalments.GenerationCompletionInstalment(_model.CompletionDate.Value, completionPayment, instalments.MaxBy(x => x.AcademicYear + x.DeliveryPeriod)!.EpisodePriceKey);
            instalments = instalments.Append(completionInstalment).ToList();
        }

        if(LastDayOfLearning.HasValue)
        {
            instalments = OnProgramPayments.RemoveAfterLastDayOfLearning(instalments, _prices, LastDayOfLearning.Value);
            additionalPayments = AdditionalPayments.RemoveAfterLastDayOfLearning(additionalPayments, LastDayOfLearning.Value);
        }

        if (_earningsProfile == null)
        {
            _earningsProfile = this.CreateEarningsProfile(onProgramTotal, instalments, additionalPayments, new List<MathsAndEnglish>(), completionPayment, ApprenticeshipEpisodeKey, true, calculationData);
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

        if (_earningsProfile.HasEvent<EarningsProfileUpdatedEvent>(x => !x.InitialGeneration)) // if earnings were updated, raise recalculated event except on initial generation, which is handled by the EarningsGenerationEvent publishing logic elsewhere (this is done here instead of in earningProfile as here we have access to the apprenticeship)
        {
            this.AddEvent(this.CreateApprenticeshipEarningsRecalculatedEvent(apprenticeship));
        }
    }

    public void CalculateShortCourseOnProgram(Apprenticeship apprenticeship, ISystemClockService systemClock, bool isApproved, string calculationData)
    {
        var onProgramPayments = ShortCoursePayments.GenerateShortCoursePayments(
            Prices.Single().AgreedPrice,
            Prices.Single().StartDate,
            Prices.Single().EndDate,
            apprenticeship.ApprenticeshipEpisodes.Single().Prices.Single().PriceKey,
            apprenticeship.ApprenticeshipEpisodes.Single().CompletionDate);

        if (_earningsProfile == null)
        {
            _earningsProfile = this.CreateEarningsProfile(
                Prices.Single().AgreedPrice,
                onProgramPayments,
                new List<AdditionalPayment>(), 
                new List<MathsAndEnglish>(), 
                0, 
                ApprenticeshipEpisodeKey,
                isApproved,
                calculationData);

            _model.EarningsProfile = _earningsProfile.GetModel();
        }
        else
        {
            _earningsProfile.Update(systemClock,
                instalments: onProgramPayments);
        }
    }

    public void UpdateWithdrawalDate(DateTime? withdrawalDate, ISystemClockService systemClock)
    {
        _model.WithdrawalDate = withdrawalDate;
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

    public void RemoveAdditionalEarnings(ISystemClockService systemClock)
    {
        _earningsProfile.Update(systemClock, additionalPayments: new List<AdditionalPayment>());
    }

    /// <summary>
    /// Updates earnings for Maths and English courses to an apprenticeship.
    /// Overwrites any existing Maths and English courses' earnings.
    /// </summary>
    public void UpdateEnglishAndMaths(List<MathsAndEnglish> mathsAndEnglishCourses, ISystemClockService systemClock)
    {
        _earningsProfile!.Update(systemClock, mathsAndEnglishCourses: mathsAndEnglishCourses);
    }

    /// <summary>
    /// Updates the completion date and earnings profile accordingly with the completion instalment and balanced instalments if necessary.
    /// </summary>
    public void UpdateCompletion(DateTime? completionDate)
    {
        _model.CompletionDate = completionDate;
    }

    public void UpdateFundingBandMaximum(int fundingBandMaximum)
    {
        _model.FundingBandMaximum = fundingBandMaximum;
    }

    public void UpdatePrices(List<Learning.Types.LearningEpisodePrice> updatedPrices)
    {
        if (this.PricesAreIdentical(updatedPrices))
            return;

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

    public void UpdatePause(DateTime? pauseDate)
    {
        _model.PauseDate = pauseDate;
    }

    public void UpdatePeriodsInLearning(List<EpisodePeriodInLearning> newPeriodsInLearning)
    {
        // Remove periods in learning that are no longer present
        foreach (var existingPeriodInLearning in _periodsInLearning.ToList())
        {
            bool stillExists = newPeriodsInLearning.Any(nb =>
                nb.StartDate == existingPeriodInLearning.StartDate &&
                nb.EndDate == existingPeriodInLearning.EndDate &&
                nb.OriginalExpectedEndDate == existingPeriodInLearning.OriginalExpectedEndDate);

            if (!stillExists)
            {
                _periodsInLearning.Remove(existingPeriodInLearning);
                _model.PeriodsInLearning.Remove(existingPeriodInLearning.GetModel());
            }
        }

        // Add new periods in learning that do not already exist
        foreach (var newPeriodInLearning in newPeriodsInLearning)
        {
            bool alreadyExists = _periodsInLearning.Any(eb =>
                eb.StartDate == newPeriodInLearning.StartDate &&
                eb.EndDate == newPeriodInLearning.EndDate);

            if (!alreadyExists)
            {
                _periodsInLearning.Add(newPeriodInLearning);
                _model.PeriodsInLearning.Add(newPeriodInLearning.GetModel());
            }
        }
    }

    // This will generate instalments and additional payments not taking into account
    // any external factors (e.g. a break in between periods of learning, these will be applied later)
    private (List<Instalment> instalments, List<AdditionalPayment> additionalPayments, decimal onProgramTotal, decimal completionPayment) GenerateBasicEarnings(Apprenticeship apprenticeship)
    {
        var onProgramPayments = OnProgramPayments.GenerateEarningsForEpisodePrices(PeriodsInLearningWithMatchedPrices, FundingBandMaximum, out var onProgramTotal, out var completionPayment);

        var instalments = onProgramPayments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.PriceKey)).ToList();

        var effectiveEndDate = LastDayOfLearning ?? _prices.Max(p => p.EndDate);

        var incentivePayments = IncentivePayments.GenerateIncentivePayments(
            AgeAtStartOfApprenticeship,
            _prices.Min(p => p.StartDate),
            effectiveEndDate,
            apprenticeship.HasEHCP,
            apprenticeship.IsCareLeaver,
            apprenticeship.CareLeaverEmployerConsentGiven,
            _periodsInLearning);

        var additionalPayments = incentivePayments.Select(x => new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.IncentiveType)).ToList();

        return (instalments, additionalPayments, onProgramTotal, completionPayment);
    }

    private (EpisodePeriodInLearning, List<PriceInPeriod>) GetPricesForPeriod(EpisodePeriodInLearning periodInLearning, List<Price> allPrices)
    {
        // Select prices that overlap with this period
        var pricesInPeriod = allPrices
            .Where(p => p.StartDate <= periodInLearning.EndDate && p.EndDate >= periodInLearning.StartDate);

        // Clip prices to the boundaries of the period
        var pricePeriods = pricesInPeriod
            .Select(price =>
            {
                var startDate = price.StartDate < periodInLearning.StartDate
                    ? periodInLearning.StartDate
                    : price.StartDate;

                var endDate = price.EndDate > periodInLearning.EndDate
                    ? periodInLearning.EndDate
                    : price.EndDate;

                return new PriceInPeriod(price, startDate, endDate, periodInLearning.OriginalExpectedEndDate);
            })
            .ToList();

        return (periodInLearning, pricePeriods);
    }

    public void UpdateAgeAtStart(DateTime dateOfBirth)
    {
        _ageAtStartOfApprenticeship = dateOfBirth.CalculateAgeAtDate(_prices.Min(x => x.StartDate));
    }
}