using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.EarningsOuterApiClient;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.LogCorrelation;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkForApprenticeships(
        this IServiceCollection services,
        ApplicationSettings settings)
    {

        services.AddDbContext<ApprenticeshipEarningsDataContext>((provider, optionsBuilder) =>
        {
            optionsBuilder
                .UseSqlServer(settings.DbConnectionString,
                sql => sql.CommandTimeout(7200));
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

        endpointConfiguration.Pipeline.Register(
            behavior: typeof(OutgoingCorrelationIdBehavior),
            description: "Populates Correlation ID for outgoing messages");

        var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
        services.AddSingleton<IMessageSession>(endpointInstance);

    }

    public static IServiceCollection AddEarningsOuterApiClient(this IServiceCollection serviceCollection, EarningOuterApiConfiguration outerConfig)
    {
        outerConfig.BaseUrl = EnsureBaseAddressFormat(outerConfig.BaseUrl);
        serviceCollection.AddScoped<IEarningsOuterApiClient, EarningsOuterApiClient.EarningsOuterApiClient>(x =>
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(outerConfig.BaseUrl);
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", outerConfig.Key);
            httpClient.DefaultRequestHeaders.Add("X-Version", "1");
            return new EarningsOuterApiClient.EarningsOuterApiClient(httpClient);
        });

        return serviceCollection;
    }

    private static string EnsureBaseAddressFormat(string baseAddress)
    {
        if (baseAddress.EndsWith('/'))
            return baseAddress;

        return baseAddress + '/';
    }
}