using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SFA.DAS.ServiceBus.Implementation;

/// <summary>
/// All messages come into here and are distributed to handlers
/// </summary>
public class QueueSubscriber : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IEnumerable<MessageHandler> _messageHandlers;
    private readonly IServiceProvider _serviceProvider;
    private readonly TaskCompletionSource _keepAlive = new();
    // TODO : Add a logger and log the message processing and errors

    internal QueueSubscriber(ServiceBusClient client, ServiceBusConfig config, IEnumerable<MessageHandler> messageHandlers, IServiceProvider serviceProvider)
    {
        _processor = client.CreateProcessor(config.QueueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 5
        });
        _messageHandlers = messageHandlers;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += HandleMessage;
        _processor.ProcessErrorAsync += HandleError;

        await _processor.StartProcessingAsync(stoppingToken);

        using var reg = stoppingToken.Register(() => _keepAlive.TrySetResult());

        await _keepAlive.Task;
    }

    private async Task HandleMessage(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();

        var enclosedType = args.Message.ApplicationProperties.TryGetValue("NServiceBus.EnclosedMessageTypes", out var type)
            ? type?.ToString()
            : null;

        if(enclosedType != null)
        {
            await ProcessMessage(args, enclosedType);
        }
        else
        {
            await args.DeadLetterMessageAsync(args.Message);
        }
        
    }

    private async Task ProcessMessage(ProcessMessageEventArgs args, string enclosedType)
    {
        var handlerMeta = _messageHandlers
            .FirstOrDefault(x => enclosedType.Contains(x.HandledEventType.FullName!));

        if (handlerMeta == null)
        {
            await args.DeadLetterMessageAsync(args.Message);
            return;
        }

        var messageType = handlerMeta.HandledEventType;

        var message = JsonSerializer.Deserialize(args.Message.Body.ToString(), messageType);


        using var scope = _serviceProvider.CreateScope();

        var handlerInterface = typeof(IHandleMessages<>).MakeGenericType(messageType);

        var handler = scope.ServiceProvider.GetService(handlerInterface);

        if (handler == null)
        {
            await args.DeadLetterMessageAsync(args.Message);
            return;
        }

        var method = handlerInterface.GetMethod("Handle");

        if(method == null)
        {
            await args.DeadLetterMessageAsync(args.Message);
            return;
        }

        try
        {
            await (Task)method.Invoke(handler, new[] { message , new MessageHandlerContext(args.CancellationToken) });
        }
        catch (Exception ex)
        {
            var foo = ex.Message;
        }

        await args.CompleteMessageAsync(args.Message);
    }

    private Task HandleError(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception);
        return Task.CompletedTask;
    }
}