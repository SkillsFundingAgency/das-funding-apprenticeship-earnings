using Azure.Identity;
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

            //var tokenProvider = TokenProvider.CreateManagedIdentityTokenProvider();
            //transport.CustomTokenProvider(tokenProvider);
            transport.CustomTokenCredential(new DefaultAzureCredential());
            transport.ConnectionString(connectionString);
            transport.RuleNameShortener(ruleNameShortener.Shorten);
            transport.Transactions(TransportTransactionMode.ReceiveOnly);

            routing?.Invoke(transport.Routing());

            return config;
        }
    }
}