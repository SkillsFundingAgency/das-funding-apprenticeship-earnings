using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;

public class EarningsProfileArchivedHandler : IDomainEventHandler<EarningsProfileArchivedEvent>
{
    private readonly IApprenticeshipRepository _repository;

    public EarningsProfileArchivedHandler(IApprenticeshipRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(EarningsProfileArchivedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
    {
        await _repository.Add(@event.EarningsProfileHistoryModel);
    }
}