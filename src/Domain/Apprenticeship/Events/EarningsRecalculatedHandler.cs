using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;

public class EarningsRecalculatedHandler : IDomainEventHandler<EarningsRecalculatedEvent>
{
    private readonly IEarningsQueryRepository _repository;
    private readonly ILogger<EarningsRecalculatedHandler> _logger;

    public EarningsRecalculatedHandler(IEarningsQueryRepository repository, ILogger<EarningsRecalculatedHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(EarningsRecalculatedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation($"EarningsRecalculatedHandler replacing query store records");
        await _repository.Replace(@event.Apprenticeship);
        _logger.LogInformation($"EarningsRecalculatedHandler done replacing query store records, elapsed {stopwatch.ElapsedMilliseconds}ms");
    }
}
