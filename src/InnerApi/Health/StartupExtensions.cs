using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Health;

public static class StartupExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services, ApplicationSettings applicationSettings)
    {
        services.AddSingleton(sp => new DbHealthCheck(applicationSettings.DbConnectionString, sp.GetService<ILogger<DbHealthCheck>>()!));
        services.AddSingleton(sp => new ServiceBusHealthCheck(applicationSettings.NServiceBusConnectionString, sp.GetService<ILogger<ServiceBusHealthCheck>>()!));

        services.AddHealthChecks()
            .AddCheck<DbHealthCheck>("Database")
            .AddCheck<ServiceBusHealthCheck>("ServiceBus");

        return services;
    }
}
