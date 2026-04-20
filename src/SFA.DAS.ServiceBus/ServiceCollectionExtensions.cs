using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.ServiceBus.Implementation;

namespace SFA.DAS.ServiceBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceBus(this IServiceCollection services, ServiceBusConfig configuration)
    {
        if (configuration.CommunicationDirection == CommunicationDirection.NotSet)
            throw new ServiceBusException("CommunicationDirection must be set to Send, Receive or Both.");

        services.AddSingleton(configuration);
        services.AddSingleton<IMessageHandlerRegistry, MessageHandlerRegistry>();

        if (configuration.UseInstallers)
            BuildInfractructure(services, configuration);

        if (ShouldReceive(configuration))
            services.AddSingleton<IFunctionEndpoint, FunctionEndpoint>();

        if(ShouldSend(configuration))
            ConfigureSend(services, configuration);

        return services;
    }

    private static void BuildInfractructure(IServiceCollection services, ServiceBusConfig configuration)
    {
        services.AddSingleton(sp =>
            new ServiceBusAdministrationClient(
                configuration.FullyQualifiedNamespace,
                new DefaultAzureCredential()));

        services.AddSingleton<IServiceBusInfrastructureBuilder, ServiceBusInfrastructureBuilder>();

        services.AddHostedService<ServiceBusStartupHostedService>();

    }

    private static void ConfigureSend(IServiceCollection services, ServiceBusConfig configuration)
    {
        if (string.IsNullOrEmpty(configuration.TopicName))
            throw new ServiceBusException("TopicName must be set when CommunicationDirection is Send or Both.");

        if(string.IsNullOrEmpty(configuration.FullyQualifiedNamespace))
            throw new ServiceBusException("FullyQualifiedNamespace must be set when CommunicationDirection is Send or Both.");

        services.AddSingleton(new ServiceBusClient(
            configuration.FullyQualifiedNamespace,
            new DefaultAzureCredential()));

        services.AddSingleton<IMessageSession, MessageSession>();
    }

    private static bool ShouldReceive(ServiceBusConfig config)
    {
        return config.CommunicationDirection == CommunicationDirection.Receive ||
            config.CommunicationDirection == CommunicationDirection.Both;
    }

    private static bool ShouldSend(ServiceBusConfig config)
    {
        return config.CommunicationDirection == CommunicationDirection.Send ||
            config.CommunicationDirection == CommunicationDirection.Both;
    }
}
