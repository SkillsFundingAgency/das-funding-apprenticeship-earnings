using Azure.Messaging.ServiceBus;

namespace SFA.DAS.ServiceBus;

public interface IFunctionEndpoint
{
    Task Process(ServiceBusReceivedMessage message, CancellationToken cancellationToken);
}
