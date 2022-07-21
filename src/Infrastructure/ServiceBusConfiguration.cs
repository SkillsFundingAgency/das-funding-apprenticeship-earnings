namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;

public class ServiceBusConfiguration
{
    public string NServiceBusConnectionString { get; set; }
    public string NServiceBusLicense { get; set; }
    public string LearningTransportStorageDirectory { get; set; }
}