using System.Diagnostics.CodeAnalysis;
using NServiceBus;
using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class RoutingSettingsExtensions
    {
        public static void AddRouting(this RoutingSettings settings)
        {
            settings.RouteToEndpoint(typeof(ApprenticeshipCreatedEvent), QueueNames.ApprovalCreated);
        }
    }
}
