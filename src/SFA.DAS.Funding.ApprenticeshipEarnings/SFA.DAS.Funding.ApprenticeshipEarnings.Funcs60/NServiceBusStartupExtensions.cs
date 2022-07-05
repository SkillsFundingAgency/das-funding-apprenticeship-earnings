using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.NServiceBus.AzureFunction.Hosting;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using System;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Funcs60
{
    public static class NServiceBusStartupExtensions
    {
        public static IServiceCollection AddNServiceBus(
            this IServiceCollection serviceCollection,
            ServiceBusConfiguration configuration)
        {
            var webBuilder = serviceCollection.AddWebJobs(_ => { });
            webBuilder.AddExecutionContextBinding();
            webBuilder.AddExtension(new NServiceBusExtensionConfigProvider());

            var endpointConfiguration = new EndpointConfiguration("sfa.das.funding.sandbox")
                    .UseMessageConventions()
                    .UseNewtonsoftJsonSerializer()
                ;

            if (configuration.NServiceBusConnectionString.Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
            {
                endpointConfiguration
                    .UseTransport<LearningTransport>()
                    .StorageDirectory(configuration.LearningTransportStorageDirectory);
                endpointConfiguration.UseLearningTransport(s => s.AddRouting());
            }
            else
            {
                endpointConfiguration
                    .UseAzureServiceBusTransport(configuration.NServiceBusConnectionString, r => r.AddRouting());
            }

            if (!string.IsNullOrEmpty(configuration.NServiceBusLicense))
            {
                endpointConfiguration.License(configuration.NServiceBusLicense);
            }

            var endpointWithExternallyManagedServiceProvider =
                EndpointWithExternallyManagedServiceProvider.Create(endpointConfiguration, serviceCollection);
            endpointWithExternallyManagedServiceProvider.Start(new UpdateableServiceProvider(serviceCollection));
            serviceCollection.AddSingleton(p => endpointWithExternallyManagedServiceProvider.MessageSession.Value);

            return serviceCollection;
        }
    }
}