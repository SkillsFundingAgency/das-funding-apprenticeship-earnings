namespace SFA.DAS.ServiceBus;

#pragma warning disable CS8618
public class ServiceBusConfig
{
    public string TopicName { get; set; }
    public string FullyQualifiedNamespace { get; set; }
    public string QueueName { get; set; }
    public CommunicationDirection CommunicationDirection { get; set; } = CommunicationDirection.NotSet;

    /// <summary>
    /// If true will build required queues, topics and subscriptions on application startup
    /// </summary>
    public bool UseInstallers { get; set; } = false;

    public int MaxDeliveryCount { get; set; } = 5;
    public bool ForwardToErrorQueue { get; set; } = false;
    public string? ErrorQueueName { get; set; }
}

public static class ServiceBusConfigExtensions
{
    public static string GetErrorQueueName(this ServiceBusConfig config)
    {
        return config.ErrorQueueName ?? $"{config.QueueName}-error";
    }
}

public enum CommunicationDirection
{
    NotSet,
    Send,
    Receive,
    Both
}