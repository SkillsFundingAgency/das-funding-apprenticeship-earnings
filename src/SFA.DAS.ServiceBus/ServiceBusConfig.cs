namespace SFA.DAS.ServiceBus;

#pragma warning disable CS8618
public class ServiceBusConfig
{
    public string TopicName { get; set; }
    public string FullyQualifiedNamespace { get; set; }
    public string QueueName { get; set; }
}
