using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.NServiceBus;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class ApprenticeshipPriceChangedEventHandler
{
    private readonly ICommandHandler<ProcessEpisodeUpdatedCommand> _processEpisodeUpdatedCommandHandler;

    public ApprenticeshipPriceChangedEventHandler(ICommandHandler<ProcessEpisodeUpdatedCommand> processEpisodeUpdatedCommandHandler)
    {
        _processEpisodeUpdatedCommandHandler = processEpisodeUpdatedCommandHandler;
    }

    [FunctionName(nameof(PriceChangeApprovedEventServiceBusTrigger))]
    public async Task PriceChangeApprovedEventServiceBusTrigger(
        [NServiceBusTrigger(Endpoint = QueueNames.PriceChangeApproved)] ApprenticeshipPriceChangedEvent apprenticeshipPriceChangedEvent,
        [DurableClient] IDurableEntityClient client,
        ILogger log)
    {
        log.LogInformation($"{nameof(PriceChangeApprovedEventServiceBusTrigger)} processing...");
        log.LogInformation("ApprenticeshipKey: {key} Received {eventName}: {eventJson}",
            apprenticeshipPriceChangedEvent.ApprenticeshipKey,
            nameof(ApprenticeshipPriceChangedEvent),
            JsonSerializer.Serialize(apprenticeshipPriceChangedEvent, new JsonSerializerOptions { WriteIndented = true }));

        await _processEpisodeUpdatedCommandHandler.Handle(new ProcessEpisodeUpdatedCommand(apprenticeshipPriceChangedEvent));
    }
}