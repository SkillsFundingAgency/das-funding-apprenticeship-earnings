using NServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Funcs31
{
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings settings)
        {
            settings.RouteToEndpoint(typeof(SampleFundingEvent), QueueNames.Sandbox);
        }
    }
}
