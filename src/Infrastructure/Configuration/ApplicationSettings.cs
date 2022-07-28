using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration
{
    public class ApplicationSettings
    {
        public string AzureWebJobsStorage { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string QueueName { get; set; }
        public string TopicPath { get; set; }
        public string NServiceBusConnectionString { get; set; }
        public string NServiceBusLicense { get; set; }
    }
}
