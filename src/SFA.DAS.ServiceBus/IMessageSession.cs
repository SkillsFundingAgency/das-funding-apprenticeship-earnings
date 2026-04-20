namespace SFA.DAS.ServiceBus;

public interface IMessageSession
{
    Task Publish(object message, CancellationToken cancellationToken = default);
}
