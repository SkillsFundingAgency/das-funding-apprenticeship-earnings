using Azure.Identity;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NServiceBus.Transport;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.AppStart;

internal static class NServiceBusConfiguration
{
    private const int ImmediateRetryCount = 3;

    internal static IHostBuilder ConfigureNServiceBusForSubscribe(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseNServiceBus((config, endpointConfiguration) =>
        {
            endpointConfiguration.AdvancedConfiguration.Conventions().SetConventions();

            // Configure custom recoverability policy
            endpointConfiguration.AdvancedConfiguration.Recoverability()
                .CustomPolicy(CustomRecoverabilityPolicy.RecoverabilityPolicy)
                .Immediate(immediate => immediate.NumberOfRetries(ImmediateRetryCount)) // Set the number of immediate retries
                .Delayed(delayed => delayed.NumberOfRetries(2).TimeIncrease(TimeSpan.FromSeconds(10))); // Set the number of delayed retries and time increase

            var value = config["ApplicationSettings:NServiceBusLicense"];
            if (!string.IsNullOrEmpty(value))
            {
                var decodedLicence = WebUtility.HtmlDecode(value);
                endpointConfiguration.AdvancedConfiguration.License(decodedLicence);
            }

            CheckCreateQueues(config);
        });

        return hostBuilder;
    }


    /// <summary>
    /// Check if the queues exist and create them if they don't
    /// </summary>
    private static void CheckCreateQueues(IConfiguration configuration)
    {
        var queueTriggers = GetQueueTriggers();

        var connectionString = configuration["ApplicationSettings:NServiceBusConnectionString"];
        var fullyQualifiedNamespace = connectionString.GetFullyQualifiedNamespace();
        var adminClient = new ServiceBusAdministrationClient(fullyQualifiedNamespace, new DefaultAzureCredential());

        foreach (var queueTrigger in queueTriggers)
        {
            var errorQueue = $"{queueTrigger.QueueName}-error";

            if (!adminClient.QueueExistsAsync(queueTrigger.QueueName).GetAwaiter().GetResult())
            {
                adminClient.CreateQueueAsync(errorQueue).GetAwaiter().GetResult();

                var queue = new CreateQueueOptions(queueTrigger.QueueName)
                {
                    ForwardDeadLetteredMessagesTo = errorQueue,
                };

                adminClient.CreateQueueAsync(queue).GetAwaiter().GetResult();
            }
            else
            {
                var queueProperties = adminClient.GetQueueAsync(queueTrigger.QueueName).GetAwaiter().GetResult();

                if (string.IsNullOrEmpty(queueProperties.Value.ForwardDeadLetteredMessagesTo))
                {
                    queueProperties.Value.ForwardDeadLetteredMessagesTo = errorQueue;

                    adminClient.UpdateQueueAsync(queueProperties.Value).GetAwaiter().GetResult();
                }
            }
        }
    }

    public static class CustomRecoverabilityPolicy
    {
        public static RecoverabilityAction RecoverabilityPolicy(RecoverabilityConfig config, ErrorContext context)
        {
            // Get the queue name from the context
            var queueName = context.ReceiveAddress;
            var errorQueueName = $"{queueName}-error";

            // Move the failed message to the individual error queue
            return RecoverabilityAction.MoveToError(errorQueueName);
        }
    }


    private static IEnumerable<ServiceBusTriggerAttribute> GetQueueTriggers()
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.GetName().FullName.Contains("SFA.DAS"));

        var queueTriggers = allAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .SelectMany(type => type.GetMethods())
            .SelectMany(method => method.GetParameters())
            .SelectMany(parameter => parameter.GetCustomAttributes(typeof(ServiceBusTriggerAttribute), false))
            .Cast<ServiceBusTriggerAttribute>();

        return queueTriggers;
    }
}
