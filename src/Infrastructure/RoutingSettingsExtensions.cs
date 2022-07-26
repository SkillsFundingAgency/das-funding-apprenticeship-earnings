using NServiceBus;
using SFA.DAS.Apprenticeships.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure
{
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings settings)
        {
            settings.RouteToEndpoint(typeof(ApprenticeshipCreatedEvent), QueueNames.ApprenticeshipCreated);
        }
    }
}
