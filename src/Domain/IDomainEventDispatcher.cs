namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public interface IDomainEventDispatcher
{
    Task Send<TDomainEvent>(TDomainEvent @event, CancellationToken cancellationToken = default(CancellationToken)) where TDomainEvent : IDomainEvent;
}
