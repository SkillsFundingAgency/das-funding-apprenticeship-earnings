using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class Apprenticeship : AggregateRoot
{
    public Apprenticeship(ApprenticeshipEntityModel apprenticeshipEntityModel)
    {
        ApprenticeshipKey = apprenticeshipEntityModel.ApprenticeshipKey;
        ApprovalsApprenticeshipId = apprenticeshipEntityModel.ApprovalsApprenticeshipId;
        Uln = apprenticeshipEntityModel.Uln;

        ApprenticeshipEpisodes = apprenticeshipEntityModel.ApprenticeshipEpisodes.Select(x => new ApprenticeshipEpisode(x)).ToList();
    }

    public Guid ApprenticeshipKey { get; }
    public long ApprovalsApprenticeshipId { get; }
    public string Uln { get; }

    public List<ApprenticeshipEpisode> ApprenticeshipEpisodes { get; }

    public void CalculateEarnings(ISystemClockService systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.CalculateEpisodeEarnings(systemClock);
        AddEvent(new EarningsCalculatedEvent(this));
    }

    public void RecalculateEarnings(ApprenticeshipEvent apprenticeshipEvent, ISystemClockService systemClock)
    {
        var episode = ApprenticeshipEpisodes.Single(x => x.ApprenticeshipEpisodeKey == apprenticeshipEvent.Episode.Key);
        episode.Update(apprenticeshipEvent.Episode);
        episode.CalculateEpisodeEarnings(systemClock);
        AddEvent(new EarningsRecalculatedEvent(this));
    }
}