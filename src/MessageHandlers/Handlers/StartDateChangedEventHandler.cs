using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using System.Text.Json;
using System.Threading.Tasks;
using NServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class StartDateChangedEventHandler(
    ICommandHandler<ProcessEpisodeUpdatedCommand> processEpisodeUpdatedCommandHandler,
    ILogger<StartDateChangedEventHandler> logger)
    : IHandleMessages<ApprenticeshipStartDateChangedEvent>
{
    public async Task Handle(ApprenticeshipStartDateChangedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{functionName} processing...", nameof(StartDateChangedEventHandler));
        logger.LogInformation("ApprenticeshipKey: {key} Received {eventName}: {eventJson}",
            message.ApprenticeshipKey,
            nameof(ApprenticeshipStartDateChangedEvent),
            JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true }));

        await processEpisodeUpdatedCommandHandler.Handle(new ProcessEpisodeUpdatedCommand(message));
    }
}