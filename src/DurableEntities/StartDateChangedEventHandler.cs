using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Text.Json;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;

public class StartDateChangedEventHandler
{
    [FunctionName(nameof(StartDateChangedEventServiceBusTrigger))]
    public async Task StartDateChangedEventServiceBusTrigger(
        [NServiceBusTrigger(Endpoint = QueueNames.PriceChangeApproved)] ApprenticeshipStartDateChangedEvent startDateChangedEvent,
        [DurableClient] IDurableEntityClient client,
        ILogger log)
    {
        log.LogInformation("{functionName} processing...", nameof(StartDateChangedEventServiceBusTrigger));
        log.LogInformation("ApprenticeshipKey: {key} Received {eventName}: {eventJson}",
            startDateChangedEvent.ApprenticeshipKey,
            nameof(ApprenticeshipStartDateChangedEvent),
            JsonSerializer.Serialize(startDateChangedEvent, new JsonSerializerOptions { WriteIndented = true }));

        var entityId = new EntityId(nameof(ApprenticeshipEntity), startDateChangedEvent.ApprenticeshipKey.ToString());
        await client.SignalEntityAsync(entityId, nameof(ApprenticeshipEntity.HandleApprenticeshipPriceChangeApprovedEvent), startDateChangedEvent);
    }
}