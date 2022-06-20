using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Container;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure
{
    public static class RoutingSettingsExtensions
    {
        //private const string NotificationsMessageHandler = "SFA.DAS.Notifications.MessageHandlers";

        public static void AddRouting(this RoutingSettings routingSettings)
        {
            //routingSettings.RouteToEndpoint(typeof(SendEmailCommand), NotificationsMessageHandler);
            //Types.RoutingSettingsExtensions.AddRouting(routingSettings);
        }
    }

    //public static class ServiceBusTriggeredEndpointConfigurationExtensions
    //{
    //    public static ServiceBusTriggeredEndpointConfiguration UseMessageConventions(this ServiceBusTriggeredEndpointConfiguration config)
    //    {
    //        config.Con
    //    }
    //}

    public static class NServiceBusExtensions
    {
        public static void StartNServiceBus(this IFunctionsHostBuilder builder, IConfiguration configuration)
        {
            builder.UseNServiceBus(config =>
            {
                var endpointName = configuration["NServiceBusEndpointName"];
                if (string.IsNullOrEmpty(endpointName))
                {
                    endpointName = "SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities";
                }

                var endpointConfiguration = new ServiceBusTriggeredEndpointConfiguration(endpointName);

                endpointConfiguration.AdvancedConfiguration
                    .UseMessageConventions()
                    .UseNewtonsoftJsonSerializer()
                    .UseOutbox(true)
                    //.UseContainer();
                    //.UseServicesBuilder(,)
                    .UseSqlServerPersistence(() => new SqlConnection(configuration["ApprenticeshipEarningsDatabase"]))
                    .UseUnitOfWork();

                if (configuration["ServiceBusConnectionString"].Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new NotImplementedException("todo fix learning transport"); //todo fix learning transport
                    //endpointConfiguration.AdvancedConfiguration
                    //    .UseTransport<LearningTransport>()
                    //    .StorageDirectory(configuration["UseLearningEndpointStorageDirectory"] ??
                    //                      Path.Combine(
                    //                          Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")),
                    //                          @"src\.learningtransport")); //todo 
                    //endpointConfiguration.AdvancedConfiguration.UseLearningTransport(s => s.AddRouting());
                }
                else
                {
                    endpointConfiguration.AdvancedConfiguration
                        .UseTransport<AzureServiceBusTransport>()
                        .ConnectionString(configuration["ServiceBusConnectionString"]);
                    //todo routing for messages
                    //endpointConfiguration.AdvancedConfiguration
                    //    .UseAzureServiceBusTransport(configuration["ServiceBusConnectionString"], r => r.AddRouting());
                }

                if (!string.IsNullOrEmpty(configuration["NServiceBusLicense"]))
                {
                    endpointConfiguration.AdvancedConfiguration.License(configuration["NServiceBusLicense"]);
                }

                var endpoint = Endpoint.Start(endpointConfiguration.AdvancedConfiguration);

                builder.Services.AddSingleton(p => endpoint)
                    .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>())
                    .AddHostedService<NServiceBusHostedService>();

                return endpointConfiguration;
            });
        }
    }
}
