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
    private readonly ServiceBusSender _errorSender;
    private readonly ServiceBusConfig _config;

    public FunctionEndpoint(
        IServiceScopeFactory scopeFactory, 
        IMessageHandlerRegistry registry, 
        ServiceBusConfig config, 
        ServiceBusClient busClient,
        ILogger<FunctionEndpoint> logger)
    {
        _scopeFactory = scopeFactory;
        _registry = registry;
        _config = config;
        _errorSender = busClient.CreateSender(config.GetErrorQueueName());
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

        try
        {
            await (Task)method.Invoke(handler, new[] { deserializedMessage, handlerContext });
        }
        catch (Exception ex) 
        {
            await HandleException(message, ex, cancellationToken);
        }

    }

    private async Task HandleException(ServiceBusReceivedMessage message, Exception ex, CancellationToken cancellationToken)
    {
        var deliveryCount = message.DeliveryCount;

        if (deliveryCount < _config.MaxDeliveryCount)
        {
            _logger.LogWarning(ex,
                "Retrying message {MessageId}. Attempt {Attempt}",
                message.MessageId,
                deliveryCount);

            throw ex; // let Azure retry
        }

        _logger.LogError(ex,
            "Moving message {MessageId} to error queue after {Attempts} attempts",
            message.MessageId,
            deliveryCount);

        if (_config.ForwardToErrorQueue)
        {
            await MoveToErrorQueue(message, ex, cancellationToken);
            // DO NOT THROW → message will be completed and new message will be created in error queue
        }
        else
        {
            throw ex;
        }        
    }

    private async Task MoveToErrorQueue(
        ServiceBusReceivedMessage message,
        Exception ex,
        CancellationToken ct)
    {
        var errorMessage = new ServiceBusMessage(message.Body)
        {
            MessageId = message.MessageId,
            Subject = message.Subject,
            ContentType = message.ContentType,
            CorrelationId = message.CorrelationId
        };

        // Copy application properties (headers)
        foreach (var prop in message.ApplicationProperties)
        {
            errorMessage.ApplicationProperties[prop.Key] = prop.Value;
        }

        // Add failure metadata
        errorMessage.ApplicationProperties["ErrorQueue"] = _config.ErrorQueueName;
        errorMessage.ApplicationProperties["ErrorReason"] = ex.Message;
        errorMessage.ApplicationProperties["ErrorExceptionType"] = ex.GetType().FullName;
        errorMessage.ApplicationProperties["ErrorStackTrace"] = ex.ToString();
        errorMessage.ApplicationProperties["ErrorOccurredAtUtc"] = DateTime.UtcNow.ToString("O");

        errorMessage.ApplicationProperties["OriginalQueue"] = _config.QueueName;
        errorMessage.ApplicationProperties["OriginalMessageId"] = message.MessageId;
        errorMessage.ApplicationProperties["DeliveryCount"] = message.DeliveryCount;


        await _errorSender.SendMessageAsync(errorMessage, ct);
    }
}