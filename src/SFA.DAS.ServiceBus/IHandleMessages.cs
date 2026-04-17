namespace SFA.DAS.ServiceBus;

#pragma warning disable CS8618
public interface IHandleMessages<T> where T : class
{
    public Task Handle(T message, IMessageHandlerContext context);
}

internal class MessageHandler
{
    public Type HandlerType { get; set; }
    public Type HandledEventType { get; set; }
}