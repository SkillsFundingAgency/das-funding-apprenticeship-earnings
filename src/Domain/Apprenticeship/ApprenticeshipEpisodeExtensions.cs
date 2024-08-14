using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public static class ApprenticeshipEpisodeExtensions
{
    public static Price GetCurrentPrice(this ApprenticeshipEpisode episode, ISystemClockService systemClock)
    {
        var price = episode?.Prices?.Find(x => x.ActualStartDate <= systemClock.UtcNow && x.PlannedEndDate >= systemClock.UtcNow);

        if (price == null)
        {
            price = episode?.Prices?.Find(x => x.ActualStartDate >= systemClock.UtcNow);
        }

        if(price == null)
            throw new InvalidOperationException("No current price found");

        return price;
    }
}