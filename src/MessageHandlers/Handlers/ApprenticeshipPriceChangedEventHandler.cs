using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using System.Text.Json;
using System.Threading.Tasks;
using NServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class LearningPriceChangedEventHandler(
    ICommandHandler<ProcessEpisodeUpdatedCommand> processEpisodeUpdatedCommandHandler,
    ILogger<LearningPriceChangedEventHandler> logger)
    : IHandleMessages<LearningPriceChangedEvent>
{
    public async Task Handle(LearningPriceChangedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation($"{nameof(LearningPriceChangedEventHandler)} processing...");
        logger.LogInformation("LearningKey: {key} Received {eventName}: {eventJson}",
            message.LearningKey,
            nameof(LearningPriceChangedEvent),
            JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true }));

        await processEpisodeUpdatedCommandHandler.Handle(new ProcessEpisodeUpdatedCommand(message), context.CancellationToken);
    }
}