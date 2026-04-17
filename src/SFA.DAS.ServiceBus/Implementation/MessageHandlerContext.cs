namespace SFA.DAS.ServiceBus.Implementation;

public class MessageHandlerContext : IMessageHandlerContext
{
    public MessageHandlerContext(CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
    }

    public CancellationToken CancellationToken { get; set; }
}
