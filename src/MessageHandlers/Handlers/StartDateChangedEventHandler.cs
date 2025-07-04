using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using System.Text.Json;
using System.Threading.Tasks;
using NServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class StartDateChangedEventHandler(
    ICommandHandler<ProcessEpisodeUpdatedCommand> processEpisodeUpdatedCommandHandler,
    ILogger<StartDateChangedEventHandler> logger)
    : IHandleMessages<LearningStartDateChangedEvent>
{
    public async Task Handle(LearningStartDateChangedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{functionName} processing...", nameof(StartDateChangedEventHandler));
        logger.LogInformation("LearningKey: {key} Received {eventName}: {eventJson}",
            message.LearningKey,
            nameof(LearningStartDateChangedEvent),
            JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true }));

        await processEpisodeUpdatedCommandHandler.Handle(new ProcessEpisodeUpdatedCommand(message), context.CancellationToken);
    }
}