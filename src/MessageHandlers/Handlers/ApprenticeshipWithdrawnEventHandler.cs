using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class ApprenticeshipWithdrawnEventHandler(
    ICommandHandler<ProcessWithdrawnApprenticeshipCommand> processWithdrawnApprenticeshipCommandHandler,
    ILogger<ApprenticeshipWithdrawnEventHandler> logger)
    : IHandleMessages<ApprenticeshipWithdrawnEvent>
{
    public async Task Handle(ApprenticeshipWithdrawnEvent message, IMessageHandlerContext context)
    {
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true });

        logger.LogInformation($"{nameof(ApprenticeshipWithdrawnEventHandler)} processing...");
        logger.LogInformation($"ApprenticeshipKey: {message.ApprenticeshipKey} Received {nameof(ApprenticeshipWithdrawnEvent)}: {json}");

        await processWithdrawnApprenticeshipCommandHandler.Handle(new ProcessWithdrawnApprenticeshipCommand(message), context.CancellationToken);
    }
}