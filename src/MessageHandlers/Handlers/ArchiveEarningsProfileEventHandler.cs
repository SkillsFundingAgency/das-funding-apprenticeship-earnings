using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveEarningsProfileCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Threading.Tasks;
using SFA.DAS.ServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class ArchiveEarningsProfileEventHandler(
    ICommandHandler<ArchiveEarningsProfileCommand> earningProfileArchiveCommandHandler,
    ILogger<ArchiveEarningsProfileEventHandler> logger)
    : IHandleMessages<EarningsProfileUpdatedEvent>
{
    public async Task Handle(EarningsProfileUpdatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{functionName} processing...", nameof(EarningsProfileUpdatedEvent));

        logger.LogInformation("EarningsProfileId: {Key} Received {EventName}",
            message.EarningsProfileId,
            nameof(EarningsProfileUpdatedEvent));

        await earningProfileArchiveCommandHandler.Handle(new ArchiveEarningsProfileCommand(message), context.CancellationToken);
    }
}