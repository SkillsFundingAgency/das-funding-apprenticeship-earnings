using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class StartDateChangedEventHandler
{
    private readonly ICommandHandler<ProcessEpisodeUpdatedCommand> _processEpisodeUpdatedCommandHandler;

    public StartDateChangedEventHandler(ICommandHandler<ProcessEpisodeUpdatedCommand> processEpisodeUpdatedCommandHandler)
    {
        _processEpisodeUpdatedCommandHandler = processEpisodeUpdatedCommandHandler;
    }

    [Function(nameof(StartDateChangedEventServiceBusTrigger))]
    public async Task StartDateChangedEventServiceBusTrigger(
        [QueueTrigger(QueueNames.StartDateChangeApproved)] ApprenticeshipStartDateChangedEvent startDateChangedEvent,
        ILogger log)
    {
        log.LogInformation("{functionName} processing...", nameof(StartDateChangedEventServiceBusTrigger));
        log.LogInformation("ApprenticeshipKey: {key} Received {eventName}: {eventJson}",
            startDateChangedEvent.ApprenticeshipKey,
            nameof(ApprenticeshipStartDateChangedEvent),
            JsonSerializer.Serialize(startDateChangedEvent, new JsonSerializerOptions { WriteIndented = true }));

        await _processEpisodeUpdatedCommandHandler.Handle(new ProcessEpisodeUpdatedCommand(startDateChangedEvent));
    }
}