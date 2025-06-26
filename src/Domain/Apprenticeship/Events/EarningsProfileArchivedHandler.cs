namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;

public class EarningsProfileArchivedHandler : IDomainEventHandler<EarningsProfileArchivedEvent>
{
    private readonly IMessageSession _messageSession;

    public EarningsProfileArchivedHandler(IMessageSession messageSession)
    {
        _messageSession = messageSession;
    }

    public async Task Handle(EarningsProfileArchivedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
    {
        await _messageSession.Publish(@event.ArchiveEarningsProfileEvent, cancellationToken);
    }
}