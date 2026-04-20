using Azure.Messaging.ServiceBus;

namespace SFA.DAS.ServiceBus;

/// <summary>
/// Handles incoming messages and dispatches them to the appropriate message handler. 
/// </summary>
public interface IFunctionEndpoint
{
    Task Process(ServiceBusReceivedMessage message, CancellationToken cancellationToken);
}
