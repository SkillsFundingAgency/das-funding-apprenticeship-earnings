using Microsoft.Extensions.Internal;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public static class ApprenticeshipExtensions
{
    public static ApprenticeshipEpisode GetCurrentEpisode(this Apprenticeship apprenticeship, ISystemClock systemClock)
    {
        var episode = apprenticeship.ApprenticeshipEpisodes.Find(x => x.Prices != null && x.Prices.Exists(price => price.ActualStartDate <= systemClock.UtcNow && price.PlannedEndDate >= systemClock.UtcNow));
        
        if(episode == null)
        {
            // if no episode is active for the current date, then there could be an episode for the apprenticeship that is yet to start
            episode = apprenticeship.ApprenticeshipEpisodes.Single(x => x.Prices != null && x.Prices.Exists(price => price.ActualStartDate >= systemClock.UtcNow));
        }

        if (episode == null)
            throw new InvalidOperationException("No current episode found");

        return episode!;
    }
}