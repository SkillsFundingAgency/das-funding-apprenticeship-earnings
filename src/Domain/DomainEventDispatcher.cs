using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<DomainEventDispatcher> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public DomainEventDispatcher(ILogger<DomainEventDispatcher> logger, IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task Send<TDomainEvent>(TDomainEvent @event, CancellationToken cancellationToken = default)
        where TDomainEvent : IDomainEvent
    {
        if (@event is IFireAndForgetDomainEvent)
        {
            _ = Task.Run(() => FireAndForget(@event, cancellationToken));
        }
        else
        {
            await DispatchHandlers(_serviceProvider, @event, cancellationToken);
        }
    }

    private async Task FireAndForget<TDomainEvent>(TDomainEvent @event, CancellationToken cancellationToken)
        where TDomainEvent : IDomainEvent
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            await DispatchHandlers(scope.ServiceProvider, @event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fire-and-forget domain event {EventType} failed", typeof(TDomainEvent).Name);
        }
    }

    private static async Task DispatchHandlers<TDomainEvent>(
        IServiceProvider serviceProvider,
        TDomainEvent @event,
        CancellationToken cancellationToken)
        where TDomainEvent : IDomainEvent
    {
        var handlers = serviceProvider.GetServices<IDomainEventHandler<TDomainEvent>>();

        foreach (var handler in handlers)
        {
            await handler.Handle(@event, cancellationToken);
        }
    }
}

