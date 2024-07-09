﻿using Microsoft.Extensions.Internal;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

internal static class ApprenticeshipEntityExtensions
{
    internal static ApprenticeshipEpisodeModel GetCurrentEpisode(this ApprenticeshipEntity apprenticeship, ISystemClockService systemClock)
    {
        var episode = apprenticeship.Model.ApprenticeshipEpisodes.Find(x => x.Prices.Exists(price => price.ActualStartDate <= systemClock.UtcNow && price.PlannedEndDate >= systemClock.UtcNow));

        if (episode == null)
        {
            // if no episode is active for the current date, then there could be an episode for the apprenticeship that is yet to start
            episode = apprenticeship.Model.ApprenticeshipEpisodes.Single(x => x.Prices.Exists(price => price.ActualStartDate >= systemClock.UtcNow));
        }

        if (episode == null)
            throw new InvalidOperationException("No current episode found");

        return episode!;
    }
}