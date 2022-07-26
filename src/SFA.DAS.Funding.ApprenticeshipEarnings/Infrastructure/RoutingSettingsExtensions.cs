using NServiceBus;
using SFA.DAS.Apprenticeships.Events;
using SFA.DAS.Funding.IntegrationTests.Infrastructure.Events;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure
{
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings settings)
        {
            settings.RouteToEndpoint(typeof(ApprenticeshipCreatedEvent), QueueNames.ApprenticeshipLearners);

            settings.RouteToEndpoint(typeof(SampleOutputEvent), QueueNames.EarningsGenerated);
        }
    }
}
