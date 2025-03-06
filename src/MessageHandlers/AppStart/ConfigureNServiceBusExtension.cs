using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.AppStart;

public static class ConfigureNServiceBusExtension
{
    public static void ConfigureNServiceBusForSend(this IServiceCollection services, string fullyQualifiedNamespace)
    {
        var endpointConfiguration = new EndpointConfiguration(Constants.EndpointName);
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.SendOnly();

        var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
        transport.CustomTokenCredential(fullyQualifiedNamespace, new DefaultAzureCredential());
        endpointConfiguration.Conventions().SetConventions();

        var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
        services.AddSingleton<IMessageSession>(endpointInstance);
    }
}
