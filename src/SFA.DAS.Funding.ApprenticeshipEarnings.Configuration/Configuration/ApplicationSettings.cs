using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Configuration.Configuration
{
    [ExcludeFromCodeCoverage]
    public class ApplicationSettings
    {
        public string AzureWebJobsStorage { get; set; } = null!;
        public string ServiceBusConnectionString { get; set; } = null!;
        public string QueueName { get; set; } = null!;
        public string TopicPath { get; set; } = null!;
        public string NServiceBusConnectionString { get; set; } = null!;
        public string NServiceBusLicense { get; set; } = null!;
        public string DbConnectionString { get; set; } = null!;
        public string SqlConnectionString { get; set; } = null!;
        public string LearningTransportStorageDirectory { get; set; } = null!;
    }
}
