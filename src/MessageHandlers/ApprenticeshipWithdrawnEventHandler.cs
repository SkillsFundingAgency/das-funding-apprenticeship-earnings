using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class ApprenticeshipWithdrawnEventHandler
{
    private readonly IProcessWithdrawnApprenticeshipCommandHandler _processWithdrawnApprenticeshipCommandHandler;

    public ApprenticeshipWithdrawnEventHandler(IProcessWithdrawnApprenticeshipCommandHandler processWithdrawnApprenticeshipCommandHandler)
    {
        _processWithdrawnApprenticeshipCommandHandler = processWithdrawnApprenticeshipCommandHandler;
    }

    [FunctionName(nameof(ApprenticeshipWithdrawnEventServiceBusTrigger))]
    public async Task ApprenticeshipWithdrawnEventServiceBusTrigger(
        [NServiceBusTrigger(Endpoint = QueueNames.ApprenticeshipWithdrawn)] ApprenticeshipWithdrawnEvent apprenticeshipWithdrawnEvent,
        [DurableClient] IDurableEntityClient client,
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