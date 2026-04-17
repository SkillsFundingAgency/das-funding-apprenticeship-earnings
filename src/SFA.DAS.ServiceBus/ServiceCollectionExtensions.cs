using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.ServiceBus.Implementation;

namespace SFA.DAS.ServiceBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceBus(this IServiceCollection services, ServiceBusConfig configuration)
    {
        services.AddSingleton(new ServiceBusClient(configuration.FullyQualifiedNamespace, new DefaultAzureCredential()));

        var messageHandlerTypes = GetMessageHandlerTypes();

        foreach (var handlerType in messageHandlerTypes)
        {
            var interfaceType = typeof(IHandleMessages<>).MakeGenericType(handlerType.HandledEventType);

            services.AddTransient(interfaceType, handlerType.HandlerType);
        }

        services.AddHostedService(sp =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return new QueueSubscriber(client, configuration, messageHandlerTypes, sp);
        });
        return services;
    }

    internal static IEnumerable<MessageHandler> GetMessageHandlerTypes()
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        var result = allAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetInterfaces()
                .Any(interfaceType =>
                    interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IHandleMessages<>)))
            .SelectMany(matchingClass => matchingClass.GetInterfaces(),
                (matchingClass, handlerInterface) => new { matchingClass, handlerInterface })
            .Where(t => t.handlerInterface.IsGenericType &&
                        t.handlerInterface.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
            .Select(t => new MessageHandler
            {
                HandlerType = t.matchingClass,
                HandledEventType = t.handlerInterface.GetGenericArguments()[0]
            }).ToList();

        return result;
    }
}
