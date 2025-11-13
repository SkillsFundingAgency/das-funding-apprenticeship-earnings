using Microsoft.Extensions.Internal;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Learning.Types;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class Apprenticeship : AggregateRoot
{
    public Apprenticeship(LearningCreatedEvent learningCreatedEvent)
    {
        _model = new ApprenticeshipModel
        {
            ApprovalsApprenticeshipId = learningCreatedEvent.ApprovalsApprenticeshipId,
            Key = learningCreatedEvent.LearningKey,
            Uln = learningCreatedEvent.Uln,
            Episodes = new List<EpisodeModel> { new EpisodeModel(learningCreatedEvent.LearningKey, learningCreatedEvent.Episode) }
        };
        _episodes = _model.Episodes.Select(this.GetEpisodeFromModel).ToList();
    }

    private Apprenticeship(ApprenticeshipModel model)
    {
        _model = model;
        _episodes = _model.Episodes.Select(this.GetEpisodeFromModel).ToList();
    }

    private ApprenticeshipModel _model;
    private readonly List<ApprenticeshipEpisode> _episodes;

    public Guid ApprenticeshipKey => _model.Key;
    public long ApprovalsApprenticeshipId => _model.ApprovalsApprenticeshipId;
    public string Uln => _model.Uln;
    public bool HasEHCP => _model?.HasEHCP ?? false;
    public bool IsCareLeaver => _model?.IsCareLeaver ?? false;
    public bool CareLeaverEmployerConsentGiven => _model?.CareLeaverEmployerConsentGiven ?? false;

    public IReadOnlyCollection<ApprenticeshipEpisode> ApprenticeshipEpisodes => new ReadOnlyCollection<ApprenticeshipEpisode>(_episodes);
    
    public static Apprenticeship Get(ApprenticeshipModel entity)
    {
        return new Apprenticeship(entity);
    }

    public ApprenticeshipModel GetModel()
    {
        return _model;
    }

    public void CalculateEarnings(ISystemClockService systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.CalculateEpisodeEarnings(this, systemClock);
    }

    public void Withdraw(DateTime withdrawalDate, ISystemClockService systemClock)
    {
        var episode = this.GetCurrentEpisode(systemClock);
        episode.Withdraw(withdrawalDate, systemClock);
    }

    public void ReverseWithdrawal(ISystemClockService systemClock)
    {
        var episode = this.GetCurrentEpisode(systemClock);
        episode.ReverseWithdrawal(systemClock);
    }

    public void UpdateCareDetails(bool hasEHCP, bool isCareLeaver, bool careLeaverEmployerConsentGiven, ISystemClockService systemClock)
    {
        if(HasEHCP == hasEHCP && IsCareLeaver == isCareLeaver && CareLeaverEmployerConsentGiven == careLeaverEmployerConsentGiven)
        {
            return;
        }

        _model.HasEHCP = hasEHCP;
        _model.IsCareLeaver = isCareLeaver;
        _model.CareLeaverEmployerConsentGiven = careLeaverEmployerConsentGiven;
        var currentEpisode = this.GetCurrentEpisode(systemClock);

        if(currentEpisode.AgeAtStartOfApprenticeship > 18) // Only recalculate if the age is 19 or older
        {
            currentEpisode.CalculateEpisodeEarnings(this, systemClock);
        }

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
    public void UpdateMathsAndEnglishCourses(List<MathsAndEnglish> mathsAndEnglishCourses, ISystemClockService systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.UpdateMathsAndEnglishCourses(mathsAndEnglishCourses, systemClock);
    }

    /// <summary>
    /// Updates completion date for the apprenticeship.
    /// Completion payment will be generated.
    /// Balancing payments will be generated if necessary.
    /// </summary>
    public void UpdateCompletion(DateTime? completionDate, ISystemClockService systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.UpdateCompletion(this, completionDate, systemClock);
        currentEpisode.ReEvaluateEarningsAfterEndOfLearning(systemClock);
    }

    public void UpdatePrices(List<LearningEpisodePrice> prices, Guid apprenticeshipEpisodeKey, int ageAtStartOfLearning, ISystemClockService systemClock)
    {
        var episode = ApprenticeshipEpisodes.Single(x => x.ApprenticeshipEpisodeKey == apprenticeshipEpisodeKey);

        if (episode.PricesAreIdentical(prices))
        {
            return;
        }

        episode.UpdatePrices(prices, ageAtStartOfLearning);
        episode.CalculateEpisodeEarnings(this, systemClock);
        episode.UpdateCompletion(this, episode.CompletionDate, systemClock);
        episode.ReEvaluateEarningsAfterEndOfLearning(systemClock);
    }

    public void Pause(DateTime? pauseDate, ISystemClockService systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.UpdatePause(pauseDate);
        currentEpisode.ReEvaluateEarningsAfterEndOfLearning(systemClock);
    }

}