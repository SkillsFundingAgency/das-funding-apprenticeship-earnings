using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace SFA.DAS.ServiceBus.Implementation;

internal class MessageSession : IMessageSession
{

    private readonly ServiceBusSender _sender;

    public MessageSession(ServiceBusClient client, ServiceBusConfig config)
    {
        _sender = client.CreateSender(config.TopicName);
    }

    public async Task Publish(object message, CancellationToken cancellationToken = default)
    {
        var messageType = message.GetType();

        var json = JsonSerializer.Serialize(message);

        var sbMessage = new ServiceBusMessage(json)
        {
            MessageId = Guid.NewGuid().ToString()
        };

        sbMessage.ApplicationProperties["NServiceBus.EnclosedMessageTypes"] = messageType.FullName;

        await _sender.SendMessageAsync(sbMessage, cancellationToken);
    }
}
