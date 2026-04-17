namespace SFA.DAS.ServiceBus;

public interface IMessageHandlerContext
{
    public CancellationToken CancellationToken { get; set; }
}
