using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.ServiceBus.Implementation;

namespace SFA.DAS.ServiceBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceBus(this IServiceCollection services, ServiceBusConfig configuration)
    {
        services.AddSingleton<IMessageHandlerRegistry, MessageHandlerRegistry>();
        services.AddSingleton<IFunctionEndpoint, FunctionEndpoint>();
        return services;
    }
}
