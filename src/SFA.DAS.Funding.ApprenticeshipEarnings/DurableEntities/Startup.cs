using System;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.InternalEvents;

[assembly: FunctionsStartup(typeof(SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Startup))]
namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities
{
    public class Startup : FunctionsStartup
    {
        private static readonly string TopicPathKey = "TopicPath";
        private static readonly string QueueNameKey = "QueueName";
        private static readonly string ServiceBusConnectionStringKey = "NServiceBusConnectionString";

        public IConfiguration Configuration { get; set; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            
            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            
            
            Configuration = configBuilder.Build();
            Environment.SetEnvironmentVariable("NServiceBusConnectionString", Configuration["NServiceBusConnectionString"], EnvironmentVariableTarget.Process);

            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), Configuration));
            builder.Services.AddNServiceBus(Configuration);

            //QueueHelper.EnsureTopic(Configuration[ServiceBusConnectionStringKey], Configuration[TopicPathKey]).GetAwaiter().GetResult();
            //QueueHelper.EnsureQueue(Configuration[ServiceBusConnectionStringKey], Configuration[QueueNameKey]).GetAwaiter().GetResult();
            //QueueHelper.EnsureSubscription(Configuration[ServiceBusConnectionStringKey], Configuration[TopicPathKey], Configuration[QueueNameKey], typeof(InternalApprenticeshipLearnerEvent)).GetAwaiter().GetResult();
        }
    }
}
