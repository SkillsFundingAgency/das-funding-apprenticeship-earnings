using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Text.Json;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;

public class ApprenticeshipPriceChangedEventHandler
{
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


        var entityId = new EntityId(nameof(ApprenticeshipEntity), apprenticeshipPriceChangedEvent.ApprenticeshipKey.ToString());
        await client.SignalEntityAsync(entityId, nameof(ApprenticeshipEntity.HandleApprenticeshipPriceChangeApprovedEvent), apprenticeshipPriceChangedEvent);
    }
}