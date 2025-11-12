using Microsoft.Extensions.Hosting;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.LogCorrelation;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Net;
using System.Security.Cryptography;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.AppStart;

internal static class NServiceBusConfiguration
{
    internal static IHostBuilder ConfigureNServiceBusForSubscribe(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseNServiceBus((config, endpointConfiguration) =>
        {
            var excludedMessageTypes = new[]
            {
                typeof(EarningsGeneratedEvent),
                typeof(ApprenticeshipEarningsRecalculatedEvent),
                typeof(LearningWithdrawnEvent)
            };

            endpointConfiguration.LogDiagnostics();

            endpointConfiguration.Transport.SubscriptionRuleNamingConvention = AzureRuleNameShortener.Shorten;
            endpointConfiguration.AdvancedConfiguration.SendFailedMessagesTo($"{Constants.EndpointName}-error");
            endpointConfiguration.AdvancedConfiguration.Conventions().SetConventions(excludedMessageTypes);

            endpointConfiguration.UseSerialization<SystemJsonSerializer>();

            endpointConfiguration.AdvancedConfiguration.EnableInstallers();

            endpointConfiguration.AdvancedConfiguration.Pipeline.Register(
                behavior: typeof(IncomingCorrelationIdBehavior),
                description: "Populates Correlation ID from incoming message");

            var value = config["ApplicationSettings:NServiceBusLicense"];
            if (!string.IsNullOrEmpty(value))
            {
                var decodedLicence = WebUtility.HtmlDecode(value);
                endpointConfiguration.AdvancedConfiguration.License(decodedLicence);
            }
        });

        return hostBuilder;
    }

    internal static class AzureRuleNameShortener
    {
        private const int AzureServiceBusRuleNameMaxLength = 50;

        public static string Shorten(Type type)
        {
            var ruleName = type.FullName;
            if (ruleName!.Length <= AzureServiceBusRuleNameMaxLength)
            {
                return ruleName;
            }

            var bytes = System.Text.Encoding.Default.GetBytes(ruleName);
            var hash = MD5.HashData(bytes);
            return new Guid(hash).ToString();
        }
    }

}
