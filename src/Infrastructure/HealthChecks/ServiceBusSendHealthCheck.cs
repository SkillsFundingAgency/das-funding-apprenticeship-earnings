using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.HealthChecks;

[ExcludeFromCodeCoverage]
public class ServiceBusSendHealthCheck : BaseHealthCheck<ServiceBusSendHealthCheck>
{
    private readonly IMessageSession _messageSession;

    public ServiceBusSendHealthCheck(IMessageSession messageSession, ILogger<ServiceBusSendHealthCheck> logger) : base(logger)
    {
        _messageSession = messageSession;
    }

    public override async Task<HealthCheckResult> HealthCheck(CancellationToken cancellationToken)
    {
        try
        {
            var message = new ServiceBusMessage("Hello, world!")
            {
                TimeToLive = TimeSpan.FromMinutes(5) // Message expires in 5 minutes
            };
            await _messageSession.Publish(message, cancellationToken);

            return HealthCheckResult.Healthy("Service bus send ok");
        }
        catch (Exception ex)
        {
            LogError("Azure Service Bus 'send' failed.", ex);
            return HealthCheckResult.Unhealthy("Azure Service Bus 'send' failed.");
        }
    }
}