using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

public class ApprenticeshipLearning : BaseLearning<ApprenticeshipLearningEntity, ApprenticeshipEpisode>
{
    private ApprenticeshipLearning(ApprenticeshipLearningEntity entity) : base(entity)
    {
        _episodes = _entity.Episodes.Select(this.GetApprenticeshipEpisodeFromEntity).ToList();
    }

    public static ApprenticeshipLearning Get(ApprenticeshipLearningEntity entity)
    {
        return new ApprenticeshipLearning(entity);
    }

    public override void Approve(Guid episodeKey, long employerAccountId, long fundingAccountId, Guid learnerKey, string learnerRef) =>
        throw new NotSupportedException("Apprenticeship approval is not yet implemented.");

    public override ApprenticeshipEpisode GetEpisode(Guid episodeKey)
    {
        var episode = _episodes.SingleOrDefault(e => e.EpisodeKey == episodeKey);
        if (episode == null)
            throw new InvalidOperationException($"No episode found for key {episodeKey}");
        return episode;
    }

    public void Remove(ISystemClockService systemClock)
    {
        this.GetCurrentEpisode(systemClock).Remove(this, systemClock);
    }

    public void Calculate(ISystemClockService systemClock, string calculationData, Guid? episodeKey = null, bool initialGenerationIsApproved = true)
    {
        ApprenticeshipEpisode episode;

        if (episodeKey.HasValue)
        {
            episode = GetEpisode(episodeKey.Value);
        }
        else
        {
            episode = this.GetCurrentEpisode(systemClock);
        }

        episode.CalculateOnProgramme(this, systemClock, calculationData, initialGenerationIsApproved);
    }

    public bool HasEpisode(Guid episodeKey)
    {
        return _episodes.Any(x => x.EpisodeKey == episodeKey);
    }

    public void AddUnapprovedEpisode(
        CreateUnapprovedApprenticeshipLearningRequest request,
        int fundingBandMaximum,
        List<ApprenticeshipPeriodInLearning> periodsInLearning)
    {
        var episodePrices = request.Prices
            .Select(price => new ApprenticeshipEpisodePriceEntity(request.EpisodeKey, price))
            .ToList();

        var periods = periodsInLearning.Any()
            ? periodsInLearning.Select(x => x.GetEntity()).ToList()
            : episodePrices.Select(x => x.ToSinglePeriodInLearning()).ToList();

        var episodeEntity = new ApprenticeshipEpisodeEntity
        {
            Key = request.EpisodeKey,
            LearningKey = request.LearningKey,
            Ukprn = request.OnProgramme.Ukprn,
            EmployerAccountId = request.OnProgramme.EmployerAccountId,
            FundingEmployerAccountId = request.OnProgramme.FundingEmployerAccountId,
            EmployerType = request.OnProgramme.EmployerType,
            LegalEntityName = request.OnProgramme.LegalEntityName,
            TrainingCode = request.OnProgramme.TrainingCode,
            FundingBandMaximum = fundingBandMaximum,
            CompletionDate = request.CompletionDate,
            WithdrawalDate = request.WithdrawalDate,
            AchievementDate = request.AchievementDate,
            PauseDate = request.PauseDate,
            Prices = episodePrices,
            PeriodsInLearning = periods
        };

        _entity.Episodes.Add(episodeEntity);
        _episodes.Add(this.GetApprenticeshipEpisodeFromEntity(episodeEntity));
    }

    public void UpdateUnapprovedApprenticeshipInformation(
        CreateUnapprovedApprenticeshipLearningRequest request,
        int fundingBandMaximum,
        List<ApprenticeshipPeriodInLearning> periodsInLearning,
        ISystemClockService systemClock)
    {
        _entity.ApprovalsApprenticeshipId = request.ApprovalsApprenticeshipId;
        _entity.Uln = request.Learner.Uln;

        UpdateDateOfBirth(request.Learner.DateOfBirth);

        var episode = GetEpisode(request.EpisodeKey);
        episode.UpdateStaticLearningDetails(
            request.OnProgramme.Ukprn,
            request.OnProgramme.EmployerAccountId,
            request.OnProgramme.FundingEmployerAccountId,
            request.OnProgramme.EmployerType,
            request.OnProgramme.TrainingCode,
            request.OnProgramme.LegalEntityName);

        episode.UpdateFundingBandMaximum(fundingBandMaximum);
        episode.UpdatePrices(request.Prices);
        episode.UpdatePeriodsInLearning(periodsInLearning);
        episode.UpdatePause(request.PauseDate);
        episode.UpdateCompletion(request.CompletionDate);
        episode.UpdateAchievementDate(request.AchievementDate);
        episode.UpdateWithdrawalDate(request.WithdrawalDate, systemClock);
        episode.UpdateAgeAtStart(_entity.DateOfBirth);

        UpdateCareDetails(
            request.Learner.Care.HasEHCP,
            request.Learner.Care.IsCareLeaver,
                request.Learner.Care.CareLeaverEmployerConsentGiven,
                systemClock);
    }

    public void UpdateCareDetails(bool hasEHCP, bool isCareLeaver, bool careLeaverEmployerConsentGiven, ISystemClockService systemClock)
    {
        if (HasEHCP == hasEHCP && IsCareLeaver == isCareLeaver && CareLeaverEmployerConsentGiven == careLeaverEmployerConsentGiven)
        {
            return;
        }

        _entity.HasEHCP = hasEHCP;
        _entity.IsCareLeaver = isCareLeaver;
        _entity.CareLeaverEmployerConsentGiven = careLeaverEmployerConsentGiven;
    }

    /// <summary>
    /// Adds additional earnings to an apprenticeship that are not included in the standard earnings calculation process.
    /// Some earnings are generated separately using this endpoint, while others are handled as part of the normal process.
    /// Note, any existing additional payments of the type being added will be removed.
    /// </summary>
    /// <param name="additionalPayments"> The additional payments to be added.</param>
    /// <param name="systemClock"> The system clock service to be used for date calculations.</param>
    public void AddAdditionalEarnings(List<AdditionalPayment> additionalPayments, string additionalPaymentType, ISystemClockService systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.AddAdditionalEarnings(additionalPayments, additionalPaymentType, systemClock);
    }

    /// <summary>
    /// Adds maths and english course earnings to an apprenticeship that are not included in the standard earnings calculation process.
    /// Maths and English course earnings are generated separately using this endpoint.
    /// Note, any existing earnings for maths and english courses will be removed.
    /// </summary>
    public void UpdateEnglishAndMathsCourses(List<EnglishAndMaths.EnglishAndMaths> englishAndMathsCourses, ISystemClockService systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.UpdateEnglishAndMaths(englishAndMathsCourses, systemClock);
    }

    public override void UpdateDateOfBirth(DateTime dateOfBirth)
    {
        _entity.DateOfBirth = dateOfBirth;
        foreach (var episode in Episodes)
        {
            episode.UpdateAgeAtStart(dateOfBirth);
        }
    }
}
