using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;

#pragma warning disable CS8618

[ExcludeFromCodeCoverage]
public class ApplicationSettings
{
    public string AzureWebJobsStorage { get; set; }
    public string ServiceBusConnectionString { get; set; }
    public string QueueName { get; set; }
    public string TopicPath { get; set; }
    public string NServiceBusConnectionString { get; set; }
    public string NServiceBusLicense { get; set; }
    public string DbConnectionString { get; set; }
    public string LearningTransportStorageDirectory { get; set; }
    public EarningOuterApiConfiguration EarningOuterApiConfiguration { get; set; }
}

[ExcludeFromCodeCoverage]
public class EarningOuterApiConfiguration
{
    public string Key { get; set; }
    public string BaseUrl { get; set; }
}
#pragma warning restore CS8618