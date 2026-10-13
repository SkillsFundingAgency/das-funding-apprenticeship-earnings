using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using System.Text.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveEnglishAndMathsEarningsProfileCommand;

public class ArchiveEnglishAndMathsEarningsProfileCommandHandler(IEarningsProfileHistoryRepository repository, ILogger<ArchiveEnglishAndMathsEarningsProfileCommandHandler> logger)
    : ICommandHandler<ArchiveEnglishAndMathsEarningsProfileCommand>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public async Task Handle(ArchiveEnglishAndMathsEarningsProfileCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{Handler} - Started", nameof(ArchiveEnglishAndMathsEarningsProfileCommandHandler));

        var updatedEvent = command.EarningsProfileForEnglishAndMathsUpdatedEvent;
        var json = JsonSerializer.Serialize(updatedEvent.EnglishAndMaths, JsonSerializerOptions);

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
