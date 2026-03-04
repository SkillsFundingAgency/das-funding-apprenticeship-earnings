using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveShortCourseEarningsProfileCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class ArchiveShortCourseEarningsProfileHandler(
    ICommandHandler<ArchiveShortCourseEarningsProfileCommand> earningProfileArchiveCommandHandler,
    ILogger<ArchiveShortCourseEarningsProfileHandler> logger)
    : IHandleMessages<ShortCourseEarningsProfileUpdatedEvent>
{
    public async Task Handle(ShortCourseEarningsProfileUpdatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{functionName} processing...", nameof(EarningsProfileUpdatedEvent));

        logger.LogInformation("EarningsProfileId: {Key} Received {EventName}",
            message.EarningsProfileId,
            nameof(EarningsProfileUpdatedEvent));

        await earningProfileArchiveCommandHandler.Handle(new ArchiveShortCourseEarningsProfileCommand(message), context.CancellationToken);
    }
}