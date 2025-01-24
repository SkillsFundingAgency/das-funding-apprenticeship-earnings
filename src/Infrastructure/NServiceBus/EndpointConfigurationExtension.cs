using Azure.Identity;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.NServiceBus
{
    public static class EndpointConfigurationExtensions
    {        
        public static EndpointConfiguration UseNewtonsoftJsonSerializer(this EndpointConfiguration config)
        {
            config.UseSerialization<global::NServiceBus.NewtonsoftJsonSerializer>();

            return config;
        }

        public static EndpointConfiguration UseMessageConventions(this EndpointConfiguration endpointConfiguration)
        {
            endpointConfiguration.Conventions()
                .DefiningMessagesAs(IsMessage)
                .DefiningEventsAs(IsEvent)
                .DefiningCommandsAs(IsCommand);

            return endpointConfiguration;
        }

        public static EndpointConfiguration UseAzureServiceBusTransport(this EndpointConfiguration config,
        string connectionString, Action<RoutingSettings> routing = null)
        {
            var transport = config.UseTransport<AzureServiceBusTransport>();
            transport.CustomTokenCredential(new DefaultAzureCredential());
            transport.ConnectionString(connectionString.FormatConnectionString());
            transport.Transactions(TransportTransactionMode.ReceiveOnly);
            transport.SubscriptionRuleNamingConvention(RuleNameShortener.Shorten);
            routing?.Invoke(transport.Routing());

            return config;
        }

        public static bool IsMessage(Type t) => IsSfaMessage(t, "Messages");

        public static bool IsEvent(Type t) => IsSfaMessage(t, "Messages.Events");

        public static bool IsCommand(Type t) => IsSfaMessage(t, "Messages.Commands");

        public static bool IsSfaMessage(Type t, string namespaceSuffix)
            => t.Namespace != null &&
                   t.Namespace.StartsWith("SFA.DAS") &&
                   t.Namespace.EndsWith(namespaceSuffix);
    }
}
