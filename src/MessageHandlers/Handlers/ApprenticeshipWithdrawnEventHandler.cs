using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class LearningWithdrawnEventHandler(
    ICommandHandler<ProcessWithdrawnApprenticeshipCommand> processWithdrawnApprenticeshipCommandHandler,
    ILogger<LearningWithdrawnEventHandler> logger)
    : IHandleMessages<LearningWithdrawnEvent>
{
    public async Task Handle(LearningWithdrawnEvent message, IMessageHandlerContext context)
    {
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true });

        logger.LogInformation($"{nameof(LearningWithdrawnEventHandler)} processing...");
        logger.LogInformation($"LearningKey: {message.LearningKey} Received {nameof(LearningWithdrawnEvent)}: {json}");

        await processWithdrawnApprenticeshipCommandHandler.Handle(new ProcessWithdrawnApprenticeshipCommand(message), context.CancellationToken);
    }
}