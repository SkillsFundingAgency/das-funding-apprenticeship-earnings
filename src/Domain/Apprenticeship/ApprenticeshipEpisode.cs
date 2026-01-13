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
    public bool IsNonLevyFullyFunded => _model.FundingType == FundingType.NonLevy && AgeAtStartOfApprenticeship < 22;
    public DateTime? CompletionDate => _model.CompletionDate;
    public DateTime? WithdrawalDate => _model.WithdrawalDate;
    public DateTime? PauseDate => _model.PauseDate;
    public decimal FundingBandMaximum => _model.FundingBandMaximum;
    public DateTime? LastDayOfLearning => this.GetLastDayOfLearning();
    public IReadOnlyCollection<PeriodInLearning> PeriodsInLearning => new ReadOnlyCollection<PeriodInLearning>(this.GetPeriodsInLearning());

    public string FundingLineType =>
        AgeAtStartOfApprenticeship < 19
            ? "16-18 Apprenticeship (Employer on App Service)"
            : "19+ Apprenticeship (Employer on App Service)";


    public void CalculateOnProgram(Apprenticeship apprenticeship, ISystemClockService systemClock)
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

        if (_earningsProfile.HasEvent<EarningsProfileUpdatedEvent>(x => !x.InitialGeneration)) // if earnings were updated, raise recalculated event except on initial generation, which is handled by the EarningsGenerationEvent publishing logic elsewhere (this is done here instead of in earningProfile as here we have access to the apprenticeship)
        {
            this.AddEvent(this.CreateApprenticeshipEarningsRecalculatedEvent(apprenticeship));
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
        var onProgramPayments = OnProgramPayments.GenerateEarningsForEpisodePrices(PeriodsInLearning, FundingBandMaximum, out var onProgramTotal, out var completionPayment);

        var instalments = onProgramPayments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.PriceKey)).ToList();

        var effectiveEndDate = LastDayOfLearning ?? _prices.Max(p => p.EndDate);

        var incentivePayments = IncentivePayments.GenerateIncentivePayments(
            AgeAtStartOfApprenticeship,
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