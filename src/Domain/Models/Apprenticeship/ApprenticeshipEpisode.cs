using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

public class ApprenticeshipEpisode : BaseEpisode<ApprenticeshipEpisodeEntity, ApprenticeshipEarningsProfile>
{
    private List<ApprenticeshipPrice> _prices;
    private List<ApprenticeshipPeriodInLearning> _periodsInLearning;

    public DateTime? PauseDate => _entity.PauseDate;
    public decimal FundingBandMaximum => _entity.FundingBandMaximum;

    public IReadOnlyCollection<ApprenticeshipPrice> Prices => new ReadOnlyCollection<ApprenticeshipPrice>(_prices);
    public IReadOnlyCollection<ApprenticeshipPeriodInLearning> EpisodePeriodsInLearning => new ReadOnlyCollection<ApprenticeshipPeriodInLearning>(_periodsInLearning);
    public List<(ApprenticeshipPeriodInLearning, List<PriceInPeriod>)> PeriodsInLearningWithMatchedPrices => EpisodePeriodsInLearning.Select(x => GetPricesForPeriod(x, Prices.ToList())).ToList(); //todo don't need this but some of the linked code (extensions) will be useful
    public DateTime? LastDayOfLearning => this.GetLastDayOfLearning();

    public bool IsNonLevyFullyFunded => _entity.FundingType == FundingType.NonLevy && AgeAtStartOfApprenticeship < 22;
    public string FundingLineType => AgeAtStartOfApprenticeship < 19
            ? "16-18 Apprenticeship (Employer on App Service)"
            : "19+ Apprenticeship (Employer on App Service)";

    private ApprenticeshipEpisode(ApprenticeshipEpisodeEntity entity, DateTime dateOfBirth, Action<AggregateComponent> addChildToRoot) : base(entity, addChildToRoot)
    {

        _prices = _entity.Prices.Select(ApprenticeshipPrice.Get).ToList();
        _periodsInLearning = _entity.PeriodsInLearning.Select(ApprenticeshipPeriodInLearning.Get).ToList();
        if (_entity.EarningsProfile != null)
        {
            _earningsProfile = this.GetEarningsProfileFromModel(_entity.EarningsProfile);
        }

        UpdateAgeAtStart(dateOfBirth);
    }

    internal static ApprenticeshipEpisode Get(ApprenticeshipLearning learning, ApprenticeshipEpisodeEntity entity)
    {
        var episode = new ApprenticeshipEpisode(entity, learning.DateOfBirth, learning.AddChildToRoot);
        return episode;
    }

    public void CalculateOnProgram(ApprenticeshipLearning learning, ISystemClockService systemClock, string calculationData)
    {
        var (instalments, additionalPayments, onProgramTotal, completionPayment) = GenerateBasicEarnings(learning);

        if (_entity.CompletionDate != null)
        {
            instalments = BalancingInstalments.BalanceInstalmentsForCompletion(_entity.CompletionDate.Value, instalments, _entity.Prices.Max(x => x.EndDate));
            var completionInstalment = CompletionInstalments.GenerationCompletionInstalment(_entity.CompletionDate.Value, completionPayment, instalments.MaxBy(x => x.AcademicYear + x.DeliveryPeriod)!.EpisodePriceKey);
            instalments = instalments.Append(completionInstalment).ToList();
        }

        if (LastDayOfLearning.HasValue)
        {
            instalments = OnProgramPayments.RemoveAfterLastDayOfLearning(instalments, EpisodePeriodsInLearning, LastDayOfLearning.Value);
            additionalPayments = AdditionalPayments.RemoveAfterLastDayOfLearning(additionalPayments, LastDayOfLearning.Value);
        }

        if (_earningsProfile == null)
        {
            _earningsProfile = this.CreateEarningsProfile(onProgramTotal, instalments, additionalPayments, new List<EnglishAndMaths.EnglishAndMaths>(), completionPayment, EpisodeKey, true, calculationData);
            _entity.EarningsProfile = _earningsProfile.GetModel();
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
            AddEvent(this.CreateApprenticeshipEarningsRecalculatedEvent(learning));
        }
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
        _earningsProfile!.Update(systemClock, additionalPayments: new List<AdditionalPayment>());
    }

    /// <summary>
    /// Updates earnings for Maths and English courses to an apprenticeship.
    /// Overwrites any existing Maths and English courses' earnings.
    /// </summary>
    public void UpdateEnglishAndMaths(List<EnglishAndMaths.EnglishAndMaths> mathsAndEnglishCourses, ISystemClockService systemClock)
    {
        _earningsProfile!.Update(systemClock, mathsAndEnglishCourses: mathsAndEnglishCourses);
    }

    public void UpdatePrices(List<LearningEpisodePrice> updatedPrices)
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
                _entity.Prices.Remove(existingPrice.GetEntity());
            }
        }

        var newPrices = updatedPrices
            .Where(x => _prices.All(y => y.PriceKey != x.Key))
            .Select(x => new ApprenticeshipPrice(x.Key, x.StartDate, x.EndDate, x.TotalPrice))
            .ToList();
        _entity.Prices.AddRange(newPrices.Select(x => x.GetEntity()));
        _prices.AddRange(newPrices);
    }

    public void UpdatePause(DateTime? pauseDate)
    {
        _entity.PauseDate = pauseDate;
    }

    public void UpdatePeriodsInLearning(List<ApprenticeshipPeriodInLearning> newPeriodsInLearning)
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
                _entity.PeriodsInLearning.Remove(existingPeriodInLearning.GetEntity());
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
                _entity.PeriodsInLearning.Add(newPeriodInLearning.GetEntity());
            }
        }
    }

    private (ApprenticeshipPeriodInLearning, List<PriceInPeriod>) GetPricesForPeriod(ApprenticeshipPeriodInLearning periodInLearning, List<ApprenticeshipPrice> allPrices)
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

    // This will generate instalments and additional payments not taking into account
    // any external factors (e.g. a break in between periods of learning, these will be applied later)
    private (List<ApprenticeshipInstalment> instalments, List<AdditionalPayment> additionalPayments, decimal onProgramTotal, decimal completionPayment) GenerateBasicEarnings(ApprenticeshipLearning apprenticeship)
    {
        var onProgramPayments = OnProgramPayments.GenerateEarningsForEpisodePrices(PeriodsInLearningWithMatchedPrices, FundingBandMaximum, out var onProgramTotal, out var completionPayment);

        var instalments = onProgramPayments.Select(x => new ApprenticeshipInstalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.PriceKey)).ToList();

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

    public void UpdateAgeAtStart(DateTime dateOfBirth)
    {
        var startDate = _prices.Min(x => x.StartDate);
        _ageAtStartOfApprenticeship = dateOfBirth.CalculateAgeAtDate(Prices.Min(x => x.StartDate));
    }

    public void UpdateFundingBandMaximum(int fundingBandMaximum)
    {
        _entity.FundingBandMaximum = fundingBandMaximum;
    }

    public override void Approve() => _earningsProfile!.Approve();
}