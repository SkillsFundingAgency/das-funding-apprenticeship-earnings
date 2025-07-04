using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;

public class EarningsCalculatedHandler : IDomainEventHandler<EarningsCalculatedEvent>
{
    private readonly IEarningsQueryRepository _repository;
    
    public EarningsCalculatedHandler(IEarningsQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(EarningsCalculatedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
    {
        await _repository.Add(@event.Apprenticeship);
    }
}
