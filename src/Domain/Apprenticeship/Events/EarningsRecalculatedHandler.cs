using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;

public class EarningsRecalculatedHandler : IDomainEventHandler<EarningsRecalculatedEvent>
{
    private readonly IEarningsQueryRepository _repository;
    
    public EarningsRecalculatedHandler(IEarningsQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(EarningsRecalculatedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
    {
        await _repository.Replace(@event.Apprenticeship);
    }
}
