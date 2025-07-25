﻿using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.LogCorrelation;

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
}
