using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration
{
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
    }
}
