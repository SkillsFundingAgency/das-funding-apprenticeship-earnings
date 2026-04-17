using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.ServiceBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceBus(this IServiceCollection services, string fullyQualifiedNamespace)
    {
        services.AddSingleton(new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential()));
        return services;
    }
}
