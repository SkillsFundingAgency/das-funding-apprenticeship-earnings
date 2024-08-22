using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Text.Json;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;

public class StartDateChangedEventHandler
{
    private readonly IProcessEpisodeUpdatedCommandHandler _processEpisodeUpdatedCommandHandler;

    public StartDateChangedEventHandler(IProcessEpisodeUpdatedCommandHandler processEpisodeUpdatedCommandHandler)
    {
        _processEpisodeUpdatedCommandHandler = processEpisodeUpdatedCommandHandler;
    }

    [FunctionName(nameof(StartDateChangedEventServiceBusTrigger))]
    public async Task StartDateChangedEventServiceBusTrigger(
        [NServiceBusTrigger(Endpoint = QueueNames.StartDateChangeApproved)] ApprenticeshipStartDateChangedEvent startDateChangedEvent,
        [DurableClient] IDurableEntityClient client,
        ILogger log)
    {
        log.LogInformation("{functionName} processing...", nameof(StartDateChangedEventServiceBusTrigger));
        log.LogInformation("ApprenticeshipKey: {key} Received {eventName}: {eventJson}",
            startDateChangedEvent.ApprenticeshipKey,
            nameof(ApprenticeshipStartDateChangedEvent),
            JsonSerializer.Serialize(startDateChangedEvent, new JsonSerializerOptions { WriteIndented = true }));

        await _processEpisodeUpdatedCommandHandler.RecalculateEarnings(new ProcessEpisodeUpdatedCommand(startDateChangedEvent));
    }
}