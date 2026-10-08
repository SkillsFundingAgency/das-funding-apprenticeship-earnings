using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using System.Text.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveEnglishAndMathsEarningsProfileCommand;


public class ArchiveEnglishAndMathsEarningsProfileCommandHandler(IEarningsProfileHistoryRepository repository, ILogger<ArchiveEnglishAndMathsEarningsProfileCommandHandler> logger)
    : ICommandHandler<ArchiveEnglishAndMathsEarningsProfileCommand>
{
    public async Task Handle(ArchiveEnglishAndMathsEarningsProfileCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{handler} - Started", nameof(ArchiveEnglishAndMathsEarningsProfileCommandHandler));

        var updatedEvent = command.EnglishAndMathsEarningsProfileUpdatedEvent;
        var json = JsonSerializer.Serialize(updatedEvent.EnglishAndMaths, new JsonSerializerOptions { WriteIndented = true });

        var history = new EnglishAndMathsEarningsProfileHistoryEntity
        {
            Key = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            EnglishAndMathsKey = updatedEvent.EnglishAndMathsKey,
            EarningsProfileId = updatedEvent.EarningsProfileId,
            State = json,
            Version = updatedEvent.Version
        };

        await repository.Add(history);
    }
}
