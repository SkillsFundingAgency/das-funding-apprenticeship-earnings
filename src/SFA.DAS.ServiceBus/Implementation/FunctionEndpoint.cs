using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SFA.DAS.ServiceBus.Implementation;

/// <summary>
/// Handles incoming messages and dispatches them to the appropriate message handler. 
/// </summary>
internal class FunctionEndpoint : IFunctionEndpoint
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessageHandlerRegistry _registry; 
    private readonly ILogger<FunctionEndpoint> _logger;

    public FunctionEndpoint(IServiceScopeFactory scopeFactory, IMessageHandlerRegistry registry, ILogger<FunctionEndpoint> logger)
    {
        _scopeFactory = scopeFactory;
        _registry = registry;
        _logger = logger;
    }

    public async Task Process(ServiceBusReceivedMessage message, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var handlerContext = new MessageHandlerContext(cancellationToken);

        var typeName = message.ApplicationProperties["NServiceBus.EnclosedMessageTypes"]?.ToString();

        var handlerType = _registry.Resolve(typeName);

        if(handlerType == null)
        {
            _logger.LogError("No handler found for message type {MessageType}", typeName);
            // TODO: consider how we want to handle this
            return;
        }

        var method = handlerType.HandlerType.GetMethod("Handle");

        var handler = ActivatorUtilities.CreateInstance(scope.ServiceProvider, handlerType.HandlerType, []);

        var deserializedMessage = JsonSerializer.Deserialize(message.Body.ToString(), handlerType.HandledEventType);

        await (Task)method.Invoke(handler, new[] { deserializedMessage, handlerContext });
    }
}