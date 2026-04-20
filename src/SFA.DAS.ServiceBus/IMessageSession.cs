namespace SFA.DAS.ServiceBus;

/// <summary>
/// Handles sending messages to the service bus
/// </summary>
public interface IMessageSession
{
    Task Publish(object message, CancellationToken cancellationToken = default);
}
