using Microsoft.Extensions.Internal;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public interface IApprenticeshipEarningsRecalculatedEventBuilder
{
    ApprenticeshipEarningsRecalculatedEvent Build(Apprenticeship apprenticeship);
}

public class ApprenticeshipEarningsRecalculatedEventBuilder : IApprenticeshipEarningsRecalculatedEventBuilder
{
    private readonly ISystemClockService _clock;

    public ApprenticeshipEarningsRecalculatedEventBuilder(ISystemClockService systemClock)
    {
        _clock = systemClock;
    }

    public ApprenticeshipEarningsRecalculatedEvent Build(Apprenticeship apprenticeship)
    {
        var currentEpisode = apprenticeship.GetCurrentEpisode(_clock);

        return new ApprenticeshipEarningsRecalculatedEvent
        {
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            DeliveryPeriods = currentEpisode.BuildDeliveryPeriods() ?? throw new ArgumentException("DeliveryPeriods"),
            EarningsProfileId = currentEpisode.EarningsProfile!.EarningsProfileId
        };
    }
}