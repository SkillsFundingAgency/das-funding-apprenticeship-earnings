using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.InternalEvents;
using SFA.DAS.NServiceBus.AzureFunction.Configuration;
using SFA.DAS.NServiceBus.AzureFunction.Hosting;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure
{
    public static class RoutingSettingsExtensions
    {
        private const string DurableEntitiesDestination = "SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities";

        public static void AddRouting(this RoutingSettings routingSettings)
        {
           // routingSettings.RouteToEndpoint(typeof(InternalApprenticeshipLearnerEvent), DurableEntitiesDestination);
            routingSettings.RouteToEndpoint(typeof(SampleEvent), QueueNames.PublishSampleEvent);
            //Types.RoutingSettingsExtensions.AddRouting(routingSettings);
        }
    }

    public static class NServiceBusExtensions
    {
        public static IServiceCollection AddNServiceBus(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {

            var webBuilder = serviceCollection.AddWebJobs(x => { });
            webBuilder.AddExecutionContextBinding();

            var endpointConfiguration = new EndpointConfiguration("sfa.das.employmentcheck")
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer()
                .UseOutbox(true)
                .UseSqlServerPersistence(() => new SqlConnection(configuration["ApprenticeshipEarningsDatabase"]))
                .UseUnitOfWork();

            if (configuration["NServiceBusConnectionString"].Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
            {
                var dir = Path.Combine(Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory()
                    .IndexOf("src", StringComparison.Ordinal)], "src\\.learningtransport");
                endpointConfiguration
                    .UseTransport<LearningTransport>()
                    .StorageDirectory(dir);
                endpointConfiguration.UseLearningTransport(s => s.AddRouting());
            }
            else
            {
                endpointConfiguration
                    .UseAzureServiceBusTransport(configuration["NServiceBusConnectionString"], r => r.AddRouting())
                    ;
            }

            if (!string.IsNullOrEmpty(configuration["NServiceBusLicense"]))
            {
                endpointConfiguration.License(configuration["NServiceBusLicense"]);
            }

            var endpointWithExternallyManagedServiceProvider = EndpointWithExternallyManagedServiceProvider.Create(endpointConfiguration, serviceCollection);
            endpointWithExternallyManagedServiceProvider.Start(new UpdateableServiceProvider(serviceCollection));
            serviceCollection.AddSingleton(p => endpointWithExternallyManagedServiceProvider.MessageSession.Value);

            return serviceCollection;
        }

        public static IServiceCollection AddNServiceBusMessageHandlers(
         this IServiceCollection serviceCollection,
         IConfiguration configuration,
         Action<NServiceBusOptions> onConfigureOptions = null)
        {
            Environment.SetEnvironmentVariable("NServiceBusConnectionString", configuration["NServiceBusConnectionString"], EnvironmentVariableTarget.Process);

            var webBuilder = serviceCollection.AddWebJobs(x => { });
            webBuilder.AddExecutionContextBinding();

            var options = new NServiceBusOptions
            {
                OnMessageReceived = context =>
                {
                    context.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out var messageType);
                    context.Headers.TryGetValue("NServiceBus.MessageId", out var messageId);
                    context.Headers.TryGetValue("NServiceBus.CorrelationId", out var correlationId);
                    context.Headers.TryGetValue("NServiceBus.OriginatingEndpoint", out var originatingEndpoint);
                },
                OnMessageErrored = (ex, context) =>
                {
                    context.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out var messageType);
                    context.Headers.TryGetValue("NServiceBus.MessageId", out var messageId);
                    context.Headers.TryGetValue("NServiceBus.CorrelationId", out var correlationId);
                    context.Headers.TryGetValue("NServiceBus.OriginatingEndpoint", out var originatingEndpoint);
                },
            };

            onConfigureOptions?.Invoke(options);

            webBuilder.AddExtension(new NServiceBusExtensionConfigProvider(options));

            return serviceCollection;
        }
    }
}
