using NServiceBus;
using SFA.DAS.Apprenticeships.Types;
using QueueNames = SFA.DAS.Funding.ApprenticeshipEarnings.Types.QueueNames;

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
