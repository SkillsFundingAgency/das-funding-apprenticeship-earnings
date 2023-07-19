using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.NServiceBus.AzureFunction.Hosting;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;

[ExcludeFromCodeCoverage]
public static class NServiceBusStartupExtensions
{
    public static IServiceCollection AddNServiceBus(
            this IServiceCollection serviceCollection,
            ApplicationSettings applicationSettings)
    {
        var webBuilder = serviceCollection.AddWebJobs(x => { });
        webBuilder.AddExecutionContextBinding();
        webBuilder.AddExtension(new NServiceBusExtensionConfigProvider());

        var endpointConfiguration = new EndpointConfiguration("SFA.DAS.Funding.ApprenticeshipEarnings")
            .UseMessageConventions()
            .UseNewtonsoftJsonSerializer();

        endpointConfiguration.SendOnly();

        if (applicationSettings.NServiceBusConnectionString.Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
        {
            var learningTransportFolder =
                Path.Combine(
                    Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)],
                    @"src\.learningtransport");
            endpointConfiguration
                .UseTransport<LearningTransport>()
                .StorageDirectory(learningTransportFolder);
            endpointConfiguration.UseLearningTransport();
            Environment.SetEnvironmentVariable("LearningTransportStorageDirectory", learningTransportFolder, EnvironmentVariableTarget.Process);
        }
        else
        {
            endpointConfiguration
                .UseAzureServiceBusTransport(applicationSettings.NServiceBusConnectionString);
        }

        if (!string.IsNullOrEmpty(applicationSettings.NServiceBusLicense))
        {
            endpointConfiguration.License(applicationSettings.NServiceBusLicense);
        }

        var endpointWithExternallyManagedServiceProvider = EndpointWithExternallyManagedServiceProvider.Create(endpointConfiguration, serviceCollection);
        endpointWithExternallyManagedServiceProvider.Start(new UpdateableServiceProvider(serviceCollection));
        serviceCollection.AddSingleton(p => endpointWithExternallyManagedServiceProvider.MessageSession.Value);

        return serviceCollection;
    }
}