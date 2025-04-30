using Microsoft.Extensions.Internal;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class Apprenticeship : AggregateRoot
{
    public Apprenticeship(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
    {
        _model = new ApprenticeshipModel
        {
            ApprovalsApprenticeshipId = apprenticeshipCreatedEvent.ApprovalsApprenticeshipId,
            Key = apprenticeshipCreatedEvent.ApprenticeshipKey,
            Uln = apprenticeshipCreatedEvent.Uln,
            Episodes = new List<EpisodeModel> { new EpisodeModel(apprenticeshipCreatedEvent.ApprenticeshipKey, apprenticeshipCreatedEvent.Episode) }
        };
        _episodes = _model.Episodes.Select(ApprenticeshipEpisode.Get).ToList();
    }

    private Apprenticeship(ApprenticeshipModel model)
    {
        _model = model;
        _episodes = _model.Episodes.Select(ApprenticeshipEpisode.Get).ToList();
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
        AddEvent(new EarningsCalculatedEvent(this));
    }

    public void RecalculateEarnings(ApprenticeshipEvent apprenticeshipEvent, ISystemClockService systemClock)
    {
        var episode = ApprenticeshipEpisodes.Single(x => x.ApprenticeshipEpisodeKey == apprenticeshipEvent.Episode.Key);
        episode.Update(apprenticeshipEvent.Episode);
        episode.CalculateEpisodeEarnings(this, systemClock);
        AddEvent(new EarningsRecalculatedEvent(this));
    }

    public void RemovalEarningsFollowingWithdrawal(DateTime lastDayOfLearning, ISystemClockService systemClock)
    {
        foreach (var episode in ApprenticeshipEpisodes)
        {
            episode.RemoveEarningsAfter(lastDayOfLearning, systemClock);
        }
        AddEvent(new EarningsRecalculatedEvent(this));
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
            AddEvent(new EarningsRecalculatedEvent(this));
        }

    }

    /// <summary>
    /// Adds additional earnings to an apprenticeship that are not included in the standard earnings calculation process.
    /// Some earnings are generated separately using this endpoint, while others are handled as part of the normal process.
    /// </summary>
    /// <param name="additionalPayments"> The additional payments to be added.</param>
    /// <param name="systemClock"> The system clock service to be used for date calculations.</param>
    /// <param name="removeAdditionalEarningOfType"> The type of existing additional earning to be removed, if any.</param>
    public void AddAdditionalEarnings(List<AdditionalPayment> additionalPayments, ISystemClockService systemClock, string? removeAdditionalEarningOfType = null)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);

        if (!string.IsNullOrEmpty(removeAdditionalEarningOfType))
        {
            currentEpisode.RemoveAdditionalEarnings(removeAdditionalEarningOfType);
        }

        currentEpisode.AddAdditionalEarnings(additionalPayments);
        AddEvent(new EarningsRecalculatedEvent(this));
    }
}