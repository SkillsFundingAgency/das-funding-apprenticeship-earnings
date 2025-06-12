using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.HealthChecks;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Funding.ApprenticeshipEarnings.Configuration.Configuration;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Health;

[ExcludeFromCodeCoverage]
public static class StartupExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services, ApplicationSettings applicationSettings, bool sqlConnectionNeedsAccessToken)
    {
        services.AddSingleton(sp => new DbHealthCheck(
            applicationSettings.DbConnectionString, 
            sp.GetService<ILogger<DbHealthCheck>>()!,
             sp.GetSqlAzureIdentityTokenProvider(sqlConnectionNeedsAccessToken)));
        
        services.AddSingleton(sp => new ServiceBusPublishHealthCheck(sp.GetService<IMessageSession>()!, sp.GetService<ILogger<ServiceBusPublishHealthCheck>>()!));

        services.AddHealthChecks()
            .AddCheck<DbHealthCheck>("Database")
            .AddCheck<ServiceBusPublishHealthCheck>("ServiceBusPublish");

        return services;
    }
}
