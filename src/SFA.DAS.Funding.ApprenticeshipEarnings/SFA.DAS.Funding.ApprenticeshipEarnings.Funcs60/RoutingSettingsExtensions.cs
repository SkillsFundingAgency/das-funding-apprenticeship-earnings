using NServiceBus;
using SFA.DAS.Funding.Events;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Funcs60
{
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings settings)
        {
            settings.RouteToEndpoint(typeof(SampleFundingEvent), QueueNames.Sandbox);
        }
    }
}
