using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.HealthChecks;
using System.Diagnostics.CodeAnalysis;

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
        
        services.AddSingleton(sp => new ServiceBusSendHealthCheck(sp.GetService<IMessageSession>()!, sp.GetService<ILogger<ServiceBusSendHealthCheck>>()!));

        services.AddHealthChecks()
            .AddCheck<DbHealthCheck>("Database")
            .AddCheck<ServiceBusSendHealthCheck>("ServiceBusSend");

        return services;
    }
}
