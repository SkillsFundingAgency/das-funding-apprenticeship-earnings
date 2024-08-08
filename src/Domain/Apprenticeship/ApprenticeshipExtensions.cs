using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public static class ApprenticeshipExtensions
{
    public static ApprenticeshipEpisode GetCurrentEpisode(this Apprenticeship apprenticeship, ISystemClockService systemClock)
    {
        var episode = apprenticeship.ApprenticeshipEpisodes.Find(x => x.Prices != null && x.Prices.Exists(price => price.StartDate <= systemClock.UtcNow && price.EndDate >= systemClock.UtcNow));
        
        if(episode == null)
        {
            // if no episode is active for the current date, then there could be an episode for the apprenticeship that is yet to start
            episode = apprenticeship.ApprenticeshipEpisodes.SingleOrDefault(x => x.Prices != null && x.Prices.Exists(price => price.StartDate >= systemClock.UtcNow));
        }

        if (episode == null)
        {
            // if no episode is active for the current date or future, then there could be an episode for the apprenticeship that has finished
            episode = apprenticeship.ApprenticeshipEpisodes.Where(x => x.Prices != null).OrderByDescending(x => x.Prices!.Select(y => y.EndDate)).First();
        }

        if (episode == null)
            throw new InvalidOperationException("No current episode found");

        return episode!;
    }
}