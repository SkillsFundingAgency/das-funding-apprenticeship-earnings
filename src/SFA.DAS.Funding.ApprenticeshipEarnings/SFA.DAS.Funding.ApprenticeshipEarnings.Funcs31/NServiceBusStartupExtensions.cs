using System;
using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.NServiceBus.AzureFunction.Configuration;
using SFA.DAS.NServiceBus.AzureFunction.Hosting;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Funcs31
{
    public static class NServiceBusStartupExtensions
    {
        public static IServiceCollection AddNServiceBus(
            this IServiceCollection serviceCollection,
            ServiceBusConfiguration configuration)
        {
            var webBuilder = serviceCollection.AddWebJobs(x => { });
            webBuilder.AddExecutionContextBinding();

            var endpointConfiguration = new EndpointConfiguration("sfa.das.employmentcheck")
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer()
                .UseOutbox(true)
                .UseSqlServerPersistence(() => new SqlConnection(configuration.DbConnectionString))
                .UseUnitOfWork();

            endpointConfiguration
                .UseAzureServiceBusTransport(configuration.NServiceBusConnectionString, r => r.AddRouting());

            var endpointWithExternallyManagedServiceProvider =
                EndpointWithExternallyManagedServiceProvider.Create(endpointConfiguration, serviceCollection);
            endpointWithExternallyManagedServiceProvider.Start(new UpdateableServiceProvider(serviceCollection));
            serviceCollection.AddSingleton(p => endpointWithExternallyManagedServiceProvider.MessageSession.Value);

            return serviceCollection;
        }

        public static IServiceCollection AddNServiceBusMessageHandlers(
            this IServiceCollection serviceCollection,
            ServiceBusConfiguration configuration)
        {
            Environment.SetEnvironmentVariable("NServiceBusConnectionString", configuration.NServiceBusConnectionString, EnvironmentVariableTarget.Process);

            var webBuilder = serviceCollection.AddWebJobs(x => { });
            webBuilder.AddExecutionContextBinding();
            webBuilder.AddExtension(new NServiceBusExtensionConfigProvider(new NServiceBusOptions()));

            return serviceCollection;
        }
    }
}
