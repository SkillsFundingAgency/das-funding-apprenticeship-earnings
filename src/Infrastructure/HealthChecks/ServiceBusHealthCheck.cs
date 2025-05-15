using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.HealthChecks;

[ExcludeFromCodeCoverage]
public class ServiceBusHealthCheck : BaseHealthCheck<ServiceBusHealthCheck>
{
    private readonly string _connectionString;

    public ServiceBusHealthCheck(string connectionString, ILogger<ServiceBusHealthCheck> logger) : base(logger)
    {
        _connectionString = connectionString;
    }

    public override async Task<HealthCheckResult> HealthCheck(CancellationToken cancellationToken)
    {
        try
        {
            var credential = new DefaultAzureCredential();
            await using var client = new ServiceBusClient(_connectionString.GetFullyQualifiedNamespace(), credential);
            var receiver = client.CreateReceiver(Constants.EndpointName);

            // Peek a message non-destructively to verify connectivity
            var message = await receiver.PeekMessageAsync();// temp cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("Connected to Azure Service Bus.");
        }
        catch (Exception ex)
        {
            LogError("Azure Service Bus check failed.", ex);
            return HealthCheckResult.Unhealthy("Azure Service Bus check failed.");
        }
    }
}