using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
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
        currentEpisode.CalculateEarnings();
        AddEvent(new EarningsCalculatedEvent(this));
    }

    public void RecalculateEarningsEpisodeUpdated(EpisodeUpdatedEvent episodeUpdatedEvent, ISystemClockService systemClock)
    {
        var episode = this.ApprenticeshipEpisodes.Single(x => x.ApprenticeshipEpisodeKey == episodeUpdatedEvent.Episode.Key);
        episode.Update(episodeUpdatedEvent.Episode);

        if (episodeUpdatedEvent is ApprenticeshipPriceChangedEvent apprenticeshipPriceChangedEvent)
        {
            var existingEarnings = episode.EarningsProfile.Instalments.Select(x => new Earning { AcademicYear = x.AcademicYear, Amount = x.Amount, DeliveryPeriod = x.DeliveryPeriod }).ToList();
            episode.RecalculateEarnings(systemClock, apprenticeshipFunding => apprenticeshipFunding.RecalculateEarnings(existingEarnings, apprenticeshipPriceChangedEvent.EffectiveFromDate));
        }
        else if (episodeUpdatedEvent is ApprenticeshipStartDateChangedEvent apprenticeshipStartDateChangedEvent)
        {
            episode.RecalculateEarnings(systemClock, apprenticeshipFunding => apprenticeshipFunding.RecalculateEarnings(new DateTime(2000,1,1))); //todo this needs to be the new start date possibly from the event
        }

        AddEvent(new EarningsRecalculatedEvent(this));
    }
}