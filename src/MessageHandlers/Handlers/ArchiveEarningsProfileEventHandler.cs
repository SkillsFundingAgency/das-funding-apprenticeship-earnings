using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.EarningProfileArchiveCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Text.Json;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class ArchiveEarningsProfileEventHandler(
    ICommandHandler<EarningProfileArchiveCommand> earningProfileArchiveCommandHandler,
    ILogger<ArchiveEarningsProfileEventHandler> logger)
    : IHandleMessages<ArchiveEarningsProfileEvent>
{
    public async Task Handle(ArchiveEarningsProfileEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{functionName} processing...", nameof(ArchiveEarningsProfileEvent));

        logger.LogInformation("EarningsProfileId: {key} Received {eventName}: {eventJson}",
            message.EarningsProfileId,
            nameof(ArchiveEarningsProfileEvent),
            JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true }));

        await earningProfileArchiveCommandHandler.Handle(new EarningProfileArchiveCommand(message), context.CancellationToken);
    }
}