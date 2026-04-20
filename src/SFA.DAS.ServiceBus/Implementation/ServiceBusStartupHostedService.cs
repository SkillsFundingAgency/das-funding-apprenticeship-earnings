using Microsoft.Extensions.Hosting;

namespace SFA.DAS.ServiceBus.Implementation;

/// <summary>
/// This is conditionally instatiated if the ServiceBusConfig has UseInstallers = true. 
/// It triggers the building of service bus infrastructure
/// </summary>
internal class ServiceBusStartupHostedService : IHostedService
{
    private readonly IServiceBusInfrastructureBuilder _initializer;

    public ServiceBusStartupHostedService(IServiceBusInfrastructureBuilder initializer)
    {
        _initializer = initializer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _initializer.EnsureAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
