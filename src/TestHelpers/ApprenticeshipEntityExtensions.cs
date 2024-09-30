using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

public static class ApprenticeshipEntityExtensions
{
    public static EpisodeModel GetCurrentEpisode(this ApprenticeshipModel apprenticeship, ISystemClockService systemClock)
    {
        var episode = apprenticeship.Episodes.Find(x => x.Prices.Exists(price => price.StartDate <= systemClock.UtcNow && price.EndDate >= systemClock.UtcNow));

        if (episode == null)
        {
            // if no episode is active for the current date, then there could be an episode for the apprenticeship that is yet to start
            episode = apprenticeship.Episodes.SingleOrDefault(x => x.Prices.Exists(price => price.StartDate >= systemClock.UtcNow));
        }

        if (episode == null)
        {
            // if no episode is active for the current date or future, then there could be an episode for the apprenticeship that has finished
            episode = apprenticeship.Episodes.OrderByDescending(x => x.Prices.Select(y => y.EndDate)).First();
        }

        if (episode == null)
            throw new InvalidOperationException("No current episode found");

        return episode!;
    }
}