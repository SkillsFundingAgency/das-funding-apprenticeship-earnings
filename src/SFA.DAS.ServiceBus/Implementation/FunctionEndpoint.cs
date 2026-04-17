using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace SFA.DAS.ServiceBus.Implementation;

internal class FunctionEndpoint : IFunctionEndpoint
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessageHandlerRegistry _registry;

    public FunctionEndpoint(IServiceScopeFactory scopeFactory, IMessageHandlerRegistry registry)
    {
        _scopeFactory = scopeFactory;
        _registry = registry;
    }

    public async Task Process(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var handlerContext = new MessageHandlerContext(cancellationToken);

        var typeName = message.ApplicationProperties["NServiceBus.EnclosedMessageTypes"]?.ToString();

        var handlerType = _registry.Resolve(typeName);

        var method = handlerType.HandlerType.GetMethod("Handle");

        var handler = ActivatorUtilities.CreateInstance(scope.ServiceProvider, handlerType.HandlerType, []);

        var deserializedMessage = JsonSerializer.Deserialize(message.Body.ToString(), handlerType.HandledEventType);

        await (Task)method.Invoke(handler, new[] { deserializedMessage, handlerContext });
    }
}