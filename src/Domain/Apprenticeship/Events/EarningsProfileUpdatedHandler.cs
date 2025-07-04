namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;

public class EarningsProfileUpdatedHandler : IDomainEventHandler<EarningsProfileUpdatedEvent>
{
    private readonly IMessageSession _messageSession;

    public EarningsProfileUpdatedHandler(IMessageSession messageSession)
    {
        _messageSession = messageSession;
    }

    public async Task Handle(EarningsProfileUpdatedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
    {
        await _messageSession.Publish(@event.ArchiveEarningsProfileEvent, cancellationToken);
    }
}