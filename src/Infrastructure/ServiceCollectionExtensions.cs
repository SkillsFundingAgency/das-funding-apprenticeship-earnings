using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.Configuration.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkForApprenticeships(this IServiceCollection services, ApplicationSettings settings, bool connectionNeedsAccessToken)
        {
            services.AddHttpContextAccessor();

            services.AddTransient<ApplicationSettings>(provider => settings);

            services.AddDbContext<ApprenticeshipEarningsDataContext>(ServiceLifetime.Transient);
            services.AddScoped(provider => new Lazy<ApprenticeshipEarningsDataContext>(provider.GetService<ApprenticeshipEarningsDataContext>()!));

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
