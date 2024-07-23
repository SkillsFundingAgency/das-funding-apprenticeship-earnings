using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests.TestHelpers;

internal static class ApprenticeshipEntityExtensions
{
    internal static ApprenticeshipEpisodeModel GetCurrentEpisode(this ApprenticeshipEntity apprenticeship, ISystemClockService systemClock)
    {
        var episode = apprenticeship.Model.ApprenticeshipEpisodes.Find(x => x.Prices.Exists(price => price.ActualStartDate <= systemClock.UtcNow && price.PlannedEndDate >= systemClock.UtcNow));

        if (episode == null)
        {
            // if no episode is active for the current date, then there could be an episode for the apprenticeship that is yet to start
            episode = apprenticeship.Model.ApprenticeshipEpisodes.SingleOrDefault(x => x.Prices.Exists(price => price.ActualStartDate >= systemClock.UtcNow));
        }

        if (episode == null)
        {
            // if no episode is active for the current date or future, then there could be an episode for the apprenticeship that has finished
            episode = apprenticeship.Model.ApprenticeshipEpisodes.OrderByDescending(x => x.Prices.Select(y => y.PlannedEndDate)).First();
        }

        if (episode == null)
            throw new InvalidOperationException("No current episode found");

        return episode!;
    }
}