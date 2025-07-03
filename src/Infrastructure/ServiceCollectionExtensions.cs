using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkForApprenticeships(
            this IServiceCollection services,
            ApplicationSettings settings,
            bool connectionNeedsAccessToken)
        {
            services.AddSingleton<ISqlAzureIdentityTokenProvider, SqlAzureIdentityTokenProvider>();

            services.AddSingleton(provider =>
                new SqlAzureIdentityAuthenticationDbConnectionInterceptor(
                    provider.GetRequiredService<ILogger<SqlAzureIdentityAuthenticationDbConnectionInterceptor>>(),
                    provider.GetRequiredService<ISqlAzureIdentityTokenProvider>(),
                    connectionNeedsAccessToken));

            services.AddDbContext<ApprenticeshipEarningsDataContext>((provider, optionsBuilder) =>
            {
                var interceptor = provider.GetRequiredService<SqlAzureIdentityAuthenticationDbConnectionInterceptor>();

                optionsBuilder
                    .UseSqlServer(settings.DbConnectionString, sql => sql.CommandTimeout(7200))
                    .AddInterceptors(interceptor);
            });

            services.AddScoped(provider =>
            {
                var context = provider.GetRequiredService<ApprenticeshipEarningsDataContext>();
                return new Lazy<ApprenticeshipEarningsDataContext>(() => context);
            });

            return services;
        }


        public static void ConfigureNServiceBusForSend(this IServiceCollection services, string fullyQualifiedNamespace)
        {
            var endpointConfiguration = new EndpointConfiguration(Constants.EndpointName);
            endpointConfiguration.UseSerialization<SystemJsonSerializer>();
            endpointConfiguration.SendOnly();

            var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
            transport.CustomTokenCredential(fullyQualifiedNamespace, new DefaultAzureCredential());
            endpointConfiguration.Conventions().SetConventions();

            var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            services.AddSingleton<IMessageSession>(endpointInstance);
        }
    }
}
