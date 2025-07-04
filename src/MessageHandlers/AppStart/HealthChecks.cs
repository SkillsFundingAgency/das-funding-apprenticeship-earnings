using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.AppStart;

public static class HealthChecks
{
    public static IServiceCollection AddFunctionHealthChecks(this IServiceCollection services, ApplicationSettings applicationSettings)
    {
        services.AddSingleton(sp => new FunctionHealthChecker(
            new DbHealthCheck(applicationSettings.DbConnectionString, sp.GetService<ILogger<DbHealthCheck>>()!),
            new ServiceBusReceiveHealthCheck(applicationSettings.NServiceBusConnectionString, sp.GetService<ILogger<ServiceBusReceiveHealthCheck>>()!),
            new ServiceBusPublishHealthCheck(sp.GetService<IMessageSession>()!, sp.GetService<ILogger<ServiceBusPublishHealthCheck>>()!)
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
    private static ServiceBusReceiveHealthCheck? _serviceBusHealthCheck;
    private static ServiceBusPublishHealthCheck? _serviceBusSendHealthCheck;

    public FunctionHealthChecker(DbHealthCheck dbHealthCheck, ServiceBusReceiveHealthCheck serviceBusHealthCheck, ServiceBusPublishHealthCheck serviceBusSendHealthCheck)
    {
        if (_dbHealthCheck == null && _serviceBusHealthCheck == null)
        {
            _dbHealthCheck = dbHealthCheck;
            _serviceBusHealthCheck = serviceBusHealthCheck;
            _serviceBusSendHealthCheck = serviceBusSendHealthCheck;
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

        var serviceBusSendResult = await _serviceBusSendHealthCheck.CheckHealthAsync(healthCheckContext, cancellationToken);
        if (serviceBusSendResult.Status != HealthStatus.Healthy)
        {
            return false;
        }

        return true;
    }
}