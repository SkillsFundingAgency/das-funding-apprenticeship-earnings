using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public interface IApprenticeshipEarningsRecalculatedEventBuilder
{
    ApprenticeshipEarningsRecalculatedEvent Build(Apprenticeship apprenticeship);
}

public class ApprenticeshipEarningsRecalculatedEventBuilder : IApprenticeshipEarningsRecalculatedEventBuilder
{
    public ApprenticeshipEarningsRecalculatedEvent Build(Apprenticeship apprenticeship)
    {
        return new ApprenticeshipEarningsRecalculatedEvent
        {
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            DeliveryPeriods = apprenticeship.BuildDeliveryPeriods() ?? throw new ArgumentException("DeliveryPeriods")
        };
    }
}