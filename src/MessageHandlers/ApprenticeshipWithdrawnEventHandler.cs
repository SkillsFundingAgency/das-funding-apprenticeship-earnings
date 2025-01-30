using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class ApprenticeshipWithdrawnEventHandler
{
    private readonly IProcessWithdrawnApprenticeshipCommandHandler _processWithdrawnApprenticeshipCommandHandler;

    public ApprenticeshipWithdrawnEventHandler(IProcessWithdrawnApprenticeshipCommandHandler processWithdrawnApprenticeshipCommandHandler)
    {
        _processWithdrawnApprenticeshipCommandHandler = processWithdrawnApprenticeshipCommandHandler;
    }

    [Function(nameof(ApprenticeshipWithdrawnEventServiceBusTrigger))]
    public async Task ApprenticeshipWithdrawnEventServiceBusTrigger(
        [QueueTrigger(QueueNames.ApprenticeshipWithdrawn)] ApprenticeshipWithdrawnEvent apprenticeshipWithdrawnEvent,
        ILogger log)
    {
        log.LogInformation($"{nameof(ApprenticeshipWithdrawnEventServiceBusTrigger)} processing...");
        log.LogInformation("ApprenticeshipKey: {key} Received {eventName}: {eventJson}",
            apprenticeshipWithdrawnEvent.ApprenticeshipKey,
            nameof(ApprenticeshipWithdrawnEvent),
            JsonSerializer.Serialize(apprenticeshipWithdrawnEvent, new JsonSerializerOptions { WriteIndented = true }));

        await _processWithdrawnApprenticeshipCommandHandler.Process(new ProcessWithdrawnApprenticeshipCommand(apprenticeshipWithdrawnEvent));
    }
}