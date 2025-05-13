using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.AppStart;

public static class HealthChecks
{
    private static string poo;

    public static IServiceCollection AddFunctionHealthChecks(this IServiceCollection services, ApplicationSettings applicationSettings)
    {
        services.AddSingleton(sp => new FunctionHealthChecker(
            new DbHealthCheck(applicationSettings.DbConnectionString, sp.GetService<ILogger<DbHealthCheck>>()!),
            new ServiceBusHealthCheck(applicationSettings.NServiceBusConnectionString, sp.GetService<ILogger<ServiceBusHealthCheck>>()!)
            ));


        return services;
    }
}

// Because the lifecycle of Azure Functions will generate singletons multiple times
// we need to store a static reference to the health check classes
// this will allow the health checks to cache their results
public class FunctionHealthChecker
{
    private static DbHealthCheck? _dbHealthCheck;
    private static ServiceBusHealthCheck? _serviceBusHealthCheck;

    public FunctionHealthChecker(DbHealthCheck dbHealthCheck, ServiceBusHealthCheck serviceBusHealthCheck)
    {
        if (_dbHealthCheck == null && _serviceBusHealthCheck == null)
        {
            _dbHealthCheck = dbHealthCheck;
            _serviceBusHealthCheck = serviceBusHealthCheck;
        }
    }

    public async Task<bool> HealthCheck(CancellationToken cancellationToken)
    {
        var healthCheckContext = new HealthCheckContext();

        var dbResult = await _dbHealthCheck.CheckHealthAsync(healthCheckContext, cancellationToken);
        if (dbResult.Status != HealthStatus.Healthy)
        {
            return false;
        }

        var serviceBusResult = await _serviceBusHealthCheck.CheckHealthAsync(healthCheckContext, cancellationToken);
        if (serviceBusResult.Status != HealthStatus.Healthy)
        {
            return false;
        }

        return true;
    }
}