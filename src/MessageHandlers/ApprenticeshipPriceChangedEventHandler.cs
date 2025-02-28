using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using System.Text.Json;
using System.Threading.Tasks;
using NServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class ApprenticeshipPriceChangedEventHandler(
    ICommandHandler<ProcessEpisodeUpdatedCommand> processEpisodeUpdatedCommandHandler,
    ILogger<ApprenticeshipPriceChangedEventHandler> logger)
    : IHandleMessages<ApprenticeshipPriceChangedEvent>
{
    public async Task Handle(ApprenticeshipPriceChangedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation($"{nameof(ApprenticeshipPriceChangedEventHandler)} processing...");
        logger.LogInformation("ApprenticeshipKey: {key} Received {eventName}: {eventJson}",
            message.ApprenticeshipKey,
            nameof(ApprenticeshipPriceChangedEvent),
            JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true }));

        await processEpisodeUpdatedCommandHandler.Handle(new ProcessEpisodeUpdatedCommand(message));
    }
}