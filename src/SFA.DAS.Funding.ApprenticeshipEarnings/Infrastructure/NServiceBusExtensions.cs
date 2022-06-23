using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.InternalEvents;
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
        public static void AddRouting(this RoutingSettings routingSettings)
        {
            routingSettings.RouteToEndpoint(typeof(InternalApprenticeshipLearnerEvent), QueueNames.ApprenticeshipLearners);
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
            webBuilder.AddExtension<NServiceBusExtensionConfigProvider>();

            var endpointName = configuration["NServiceBusEndpointName"];
            if (string.IsNullOrEmpty(endpointName))
            {
                endpointName = QueueNames.ApprenticeshipLearners;
            }

            var endpointConfiguration = new EndpointConfiguration(endpointName)
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer()
                .UseOutbox(true)
                .UseSqlServerPersistence(() => new SqlConnection(configuration["ApprenticeshipEarningsDatabase"]))
                .UseUnitOfWork();
                
            //todo scrap this before PR?
            //endpointConfiguration.DefineCriticalErrorAction(async context =>
            //{
            //    await Console.Out.WriteLineAsync("Critical error: " + context.Exception);
            //});

            //endpointConfiguration.CustomDiagnosticsWriter(async x =>
            //{
            //    await Console.Out.WriteLineAsync("Diagnostics: " + x);
            //});

            if (configuration["NServiceBusConnectionString"].Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
            {
                endpointConfiguration
                    .UseTransport<LearningTransport>()
                    .StorageDirectory(configuration.GetValue("UseLearningEndpointStorageDirectory", Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), @"src\.learningtransport")));
                endpointConfiguration.UseLearningTransport(s => s.AddRouting());
            }
            else
            {
                endpointConfiguration
                    .UseAzureServiceBusTransport(configuration["NServiceBusConnectionString"], r => r.AddRouting());
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
    }
}
