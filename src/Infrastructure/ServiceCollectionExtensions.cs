using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.EarningsOuterApiClient;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.LogCorrelation;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

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


    public static void ConfigureNServiceBusForSend(this IServiceCollection services, string fullyQualifiedNamespace, bool useWebSockets)
    {
        var endpointConfiguration = new EndpointConfiguration(Constants.EndpointName);
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.SendOnly();

        var transport = new AzureServiceBusTransport(
           fullyQualifiedNamespace,
           new DefaultAzureCredential());

        transport.UseWebSockets = useWebSockets;
        
        endpointConfiguration.UseTransport(transport);

        endpointConfiguration.Conventions().SetConventions();

        endpointConfiguration.Pipeline.Register(
            behavior: typeof(OutgoingCorrelationIdBehavior),
            description: "Populates Correlation ID for outgoing messages");

        endpointConfiguration.AssemblyScanner().ScanFileSystemAssemblies = false;

        var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
        services.AddSingleton<IMessageSession>(endpointInstance);
    }

    public static IServiceCollection AddEarningsOuterApiClient(this IServiceCollection serviceCollection, EarningOuterApiConfiguration outerConfig)
    {
        outerConfig.BaseUrl = EnsureBaseAddressFormat(outerConfig.BaseUrl);

        serviceCollection.AddHttpClient<IEarningsOuterApiClient, EarningsOuterApiClient.EarningsOuterApiClient>(client =>
        {
            client.BaseAddress = new Uri(outerConfig.BaseUrl);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", outerConfig.Key);
            client.DefaultRequestHeaders.Add("X-Version", "1");
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