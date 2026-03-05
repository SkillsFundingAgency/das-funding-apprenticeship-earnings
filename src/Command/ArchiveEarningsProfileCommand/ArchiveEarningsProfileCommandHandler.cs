using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using System.Text.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveEarningsProfileCommand;


public class ArchiveEarningsProfileCommandHandler(IEarningsProfileHistoryRepository repository, ILogger<ArchiveEarningsProfileCommandHandler> logger)
    : ICommandHandler<ArchiveEarningsProfileCommand>
{
    public async Task Handle(ArchiveEarningsProfileCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{handler} - Started", nameof(ArchiveEarningsProfileCommandHandler));

        var json = JsonSerializer.Serialize(command.EarningsProfileUpdatedEvent, new JsonSerializerOptions { WriteIndented = true });

        var history = new ApprenticeshipEarningsProfileHistoryEntity
        {
            Key = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            EarningsProfileId = command.EarningsProfileUpdatedEvent.EarningsProfileId,
            State = json,
            Version = command.EarningsProfileUpdatedEvent.Version
        };

        await repository.Add(history);
    }
}