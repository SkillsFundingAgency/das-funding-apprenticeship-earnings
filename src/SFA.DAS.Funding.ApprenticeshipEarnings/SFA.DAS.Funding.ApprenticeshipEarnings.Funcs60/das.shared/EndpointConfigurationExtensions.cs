//using System;
//using System.Data.Common;
//using System.Text.RegularExpressions;
//using NServiceBus;

//namespace SFA.DAS.Funding.ApprenticeshipEarnings.Funcs60.das.shared
//{
//    public static class EndpointConfigurationExtensions
//    {
//        public static EndpointConfiguration UseNewtonsoftJsonSerializer(this EndpointConfiguration config)
//        {
//            config.UseSerialization<NewtonsoftSerializer>();

//            return config;
//        }

//        public static EndpointConfiguration UseErrorQueue(this EndpointConfiguration config, string errorQueue)
//        {
//            config.SendFailedMessagesTo(errorQueue);

//            return config;
//        }

//        public static EndpointConfiguration UseHeartbeat(this EndpointConfiguration config)
//        {
//            config.SendHeartbeatTo("heartbeat");

//            return config;
//        }

//        public static EndpointConfiguration UseInstallers(this EndpointConfiguration config)
//        {
//            config.EnableInstallers();

//            return config;
//        }

//        public static EndpointConfiguration UseLearningTransport(this EndpointConfiguration config, Action<RoutingSettings> routing = null)
//        {
//            var transport = config.UseTransport<LearningTransport>();
                
//            transport.Transactions(TransportTransactionMode.ReceiveOnly);

//            routing?.Invoke(transport.Routing());

//            return config;
//        }
        
//        public static EndpointConfiguration UseLicense(this EndpointConfiguration config, string licenseText)
//        {
//            config.License(licenseText);

//            return config;
//        }
        
//        public static EndpointConfiguration UseMessageConventions(this EndpointConfiguration config)
//        {
//            var conventions = config.Conventions();
            
//#pragma warning disable 618
//            conventions.DefiningCommandsAs(t => Regex.IsMatch(t.Name, @"Command(V\d+)?$") || typeof(Command).IsAssignableFrom(t));
//            conventions.DefiningEventsAs(t => Regex.IsMatch(t.Name, @"Event(V\d+)?$") || typeof(Event).IsAssignableFrom(t));
//#pragma warning restore 618

//            return config;
//        }

//        public static EndpointConfiguration UseMetrics(this EndpointConfiguration config)
//        {
//            var metrics = config.EnableMetrics();

//            metrics.SendMetricDataToServiceControl("particular.monitoring", TimeSpan.FromSeconds(10));

//            return config;
//        }

//        public static EndpointConfiguration UsePurgeOnStartup(this EndpointConfiguration config)
//        {
//            config.PurgeOnStartup(true);

//            return config;
//        }

//        public static EndpointConfiguration UseSendOnly(this EndpointConfiguration config)
//        {
//            config.SendOnly();

//            return config;
//        }

//        public static EndpointConfiguration UseOutbox(this EndpointConfiguration config, bool enableCleanup = false, TimeSpan? cleanupFrequency = null, TimeSpan? cleanupMaxAge = null)
//        {
//            var outbox = config.EnableOutbox();

//            if (!enableCleanup)
//            {
//                outbox.DisableCleanup();
//            }

//            if (cleanupFrequency != null)
//            {
//                outbox.RunDeduplicationDataCleanupEvery(cleanupFrequency.Value);
//            }

//            if (cleanupMaxAge != null)
//            {
//                outbox.KeepDeduplicationDataFor(cleanupMaxAge.Value);
//            }

//            config.EnableFeature<SqlServerClientOutboxFeature>();

//            return config;
//        }

//        public static EndpointConfiguration UseSqlServerPersistence(this EndpointConfiguration config, Func<DbConnection> connectionBuilder)
//        {
//            var persistence = config.UsePersistence<SqlPersistence>();

//            persistence.ConnectionBuilder(connectionBuilder);
//            persistence.DisableInstaller();
//            persistence.SqlDialect<SqlDialect.MsSqlServer>();
//            persistence.TablePrefix("");

//            return config;
//        }
//    }
//}