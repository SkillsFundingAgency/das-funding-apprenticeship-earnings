using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration
{
    public static class EnvironmentVariables
    {
        public static readonly string NServiceBusConnectionString = Environment.GetEnvironmentVariable("NServiceBusConnectionString");
        public static readonly string NServiceBusLicense = Environment.GetEnvironmentVariable("NServiceBusLicense");
        public static readonly string LearningTransportStorageDirectory = Environment.GetEnvironmentVariable("LearningTransportStorageDirectory");
    }
}
