namespace SFA.DAS.ServiceBus;

public class ServiceBusException : Exception
{
    public ServiceBusException(string? message) : base(message)
    {
    }
}
