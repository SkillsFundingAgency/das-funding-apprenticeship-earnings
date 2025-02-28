using Microsoft.Extensions.Hosting;
using NServiceBus;
using System;
using System.Net;
using System.Security.Cryptography;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.AppStart;

internal static class NServiceBusConfiguration
{
    internal static IHostBuilder ConfigureNServiceBusForSubscribe(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseNServiceBus((config, endpointConfiguration) =>
        {
            endpointConfiguration.LogDiagnostics();

            endpointConfiguration.Transport.SubscriptionRuleNamingConvention = AzureRuleNameShortener.Shorten;
            endpointConfiguration.AdvancedConfiguration.SendFailedMessagesTo("SFA.DAS.Funding.ApprenticeshipEarnings-error");
            endpointConfiguration.AdvancedConfiguration.Conventions()
                .DefiningEventsAs(IsEvent)
                .DefiningCommandsAs(IsCommand)
                .DefiningMessagesAs(IsMessage);

            endpointConfiguration.UseSerialization<SystemJsonSerializer>();

            endpointConfiguration.AdvancedConfiguration.EnableInstallers();

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

    private static bool IsMessage(Type t) => t.Name.EndsWith("Message");

    private static bool IsEvent(Type t) => t.Name.EndsWith("Event");

    private static bool IsCommand(Type t) => t.Name.EndsWith("Command");
}
