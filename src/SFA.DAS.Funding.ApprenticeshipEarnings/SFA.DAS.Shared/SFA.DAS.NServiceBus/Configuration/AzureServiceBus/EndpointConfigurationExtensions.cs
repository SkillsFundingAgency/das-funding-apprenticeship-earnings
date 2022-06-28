using Microsoft.Azure.ServiceBus.Primitives;
using NServiceBus;

namespace SFA.DAS.NServiceBus.Configuration.AzureServiceBus
{
    public static class EndpointConfigurationExtensions
    {
        public static EndpointConfiguration UseAzureServiceBusTransport(this EndpointConfiguration config, string connectionString, Action<RoutingSettings> routing = null)
        {
            var transport = config.UseTransport<AzureServiceBusTransport>();
            var ruleNameShortener = new RuleNameShortener();

            var tokenProvider = TokenProvider.CreateManagedIdentityTokenProvider();
            transport.CustomTokenProvider(tokenProvider);
            transport.ConnectionString(connectionString);
            transport.RuleNameShortener(ruleNameShortener.Shorten);
            transport.Transactions(TransportTransactionMode.ReceiveOnly);

            routing?.Invoke(transport.Routing());

            return config;
        }
    }
}
//namespace NServiceBus
//{
//    using Configuration.AdvancedExtensibility;
//    using Microsoft.Azure.ServiceBus.Primitives;
//    using Transport.AzureServiceBus;

//    /// <summary>
//    /// Adds access to the Azure Service Bus transport config to the global Transport object.
//    /// </summary>
//    public static class AzureServiceBusTransportSettingsExtensions
//        {
//            /// <summary>
//            /// Overrides the default token provider with a custom implementation.
//            /// </summary>
//            /// <param name="transportExtensions"></param>
//            /// <param name="tokenProvider">The token provider to be used.</param>
//            public static TransportExtensions<AzureServiceBusTransport> CustomTokenProvider(this TransportExtensions<AzureServiceBusTransport> transportExtensions, ITokenProvider tokenProvider)
//            {
//                transportExtensions.GetSettings().Set(SettingsKeys.CustomTokenProvider, tokenProvider);

//                return transportExtensions;
//            }

//        }
//    }

//    namespace NServiceBus.Transport.AzureServiceBus
//{
//    internal static class SettingsKeys
//        {
//            public const string TopicName = "AzureServiceBus.TopicName";
//            public const string MaximumSizeInGB = "AzureServiceBus.MaximumSizeInGB";
//            public const string EnablePartitioning = "AzureServiceBus.EnablePartitioning";
//            public const string PrefetchMultiplier = "AzureServiceBus.PrefetchMultiplier";
//            public const string PrefetchCount = "AzureServiceBus.PrefetchCount";
//            public const string TimeToWaitBeforeTriggeringCircuitBreaker = "AzureServiceBus.TimeToWaitBeforeTriggeringCircuitBreaker";
//            public const string SubscriptionNameShortener = "AzureServiceBus.SubscriptionNameShortener";
//            public const string RuleNameShortener = "AzureServiceBus.RuleNameShortener";
//            public const string TransportType = "AzureServiceBus.TransportType";
//            public const string CustomTokenProvider = "AzureServiceBus.CustomTokenProvider";
//            public const string CustomRetryPolicy = "AzureServiceBus.CustomRetryPolicy";
//        }
//    }
