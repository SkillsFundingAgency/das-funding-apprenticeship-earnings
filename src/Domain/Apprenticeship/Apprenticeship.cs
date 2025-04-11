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
        _model.HasEHCP = hasEHCP;
        _model.IsCareLeaver = isCareLeaver;
        _model.CareLeaverEmployerConsentGiven = careLeaverEmployerConsentGiven;
        this.GetCurrentEpisode(systemClock).CalculateEpisodeEarnings(this, systemClock);
        AddEvent(new EarningsRecalculatedEvent(this));
    }
}