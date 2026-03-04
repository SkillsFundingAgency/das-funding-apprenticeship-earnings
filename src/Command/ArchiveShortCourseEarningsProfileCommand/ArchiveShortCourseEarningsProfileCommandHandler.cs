using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using System.Text.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveShortCourseEarningsProfileCommand;


public class ArchiveShortCourseEarningsProfileCommandHandler(IEarningsProfileHistoryRepository repository, ILogger<ArchiveShortCourseEarningsProfileCommandHandler> logger)
    : ICommandHandler<ArchiveShortCourseEarningsProfileCommand>
{
    public async Task Handle(ArchiveShortCourseEarningsProfileCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{handler} - Started", nameof(ArchiveShortCourseEarningsProfileCommandHandler));

        var json = JsonSerializer.Serialize(command.EarningsProfileUpdatedEvent, new JsonSerializerOptions { WriteIndented = true });

        var history = new ShortCourseEarningsProfileHistoryEntity
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