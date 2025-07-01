using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public static class LearningEpisodeExtensions
{
    public static Price GetCurrentPrice(this LearningEpisode episode, ISystemClockService systemClock)
    {
        var price = episode?.Prices?.FirstOrDefault(x => x.StartDate <= systemClock.UtcNow && x.EndDate >= systemClock.UtcNow);

        if (price == null)
        {
            price = episode?.Prices?.FirstOrDefault(x => x.StartDate >= systemClock.UtcNow);
        }

        if(price == null)
            throw new InvalidOperationException("No current price found");

        return price;
    }
}