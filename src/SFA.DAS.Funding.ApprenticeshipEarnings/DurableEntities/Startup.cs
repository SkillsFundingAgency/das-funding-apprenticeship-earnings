using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;

[assembly: FunctionsStartup(typeof(SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Startup))]
namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities
{
    public class Startup : FunctionsStartup
    {
        private static readonly string TopicPathKey = "TopicPath";
        private static readonly string QueueNameKey = "QueueName";
        private static readonly string ServiceBusConnectionStringKey = "ServiceBusConnectionString";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            //QueueHelper.EnsureTopic(config[ServiceBusConnectionStringKey], config[TopicPathKey]).GetAwaiter().GetResult();
            //QueueHelper.EnsureQueue(config[ServiceBusConnectionStringKey], config[QueueNameKey]).GetAwaiter().GetResult();
            //QueueHelper.EnsureSubscription(config[ServiceBusConnectionStringKey], config[TopicPathKey], config[QueueNameKey], typeof(InternalApprenticeshipLearnerEvent)).GetAwaiter().GetResult();
        }
    }
}
