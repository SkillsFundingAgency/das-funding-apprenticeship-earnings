using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities
{
    public class EarningsFunctions
    {
        [FunctionName(nameof(ApprenticeshipLearnerEventServiceBusTrigger))]
        public async Task ApprenticeshipLearnerEventServiceBusTrigger(
            [NServiceBusTrigger(Endpoint = QueueNames.ApprovalCreated)] ApprenticeshipCreatedEvent apprenticeshipCreatedEvent,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            try
            {
                log.LogInformation($"{nameof(ApprenticeshipLearnerEventServiceBusTrigger)} processing...");

                log.LogInformation("ApprenticeshipKey: {0} Received ApprenticeshipCreatedEvent: {1}",
                    apprenticeshipCreatedEvent.ApprenticeshipKey,
                    JsonSerializer.Serialize(apprenticeshipCreatedEvent, new JsonSerializerOptions { WriteIndented = true }));

                if (!(apprenticeshipCreatedEvent.FundingPlatform.HasValue && apprenticeshipCreatedEvent.FundingPlatform.Value == FundingPlatform.DAS))
                {
                    log.LogInformation($"{nameof(ApprenticeshipLearnerEventServiceBusTrigger)} - Not generating earnings for non pilot apprenticeship with ApprenticeshipKey = {apprenticeshipCreatedEvent.ApprenticeshipKey}");
                    return;
                }

                var entityId = new EntityId(nameof(ApprenticeshipEntity), apprenticeshipCreatedEvent.ApprenticeshipKey.ToString());

                await client.SignalEntityAsync(entityId, nameof(ApprenticeshipEntity.HandleApprenticeshipLearnerEvent), apprenticeshipCreatedEvent);

                log.LogInformation($"Started {nameof(ApprenticeshipEntity)} with EntityId = '{entityId}'.");
            }
            catch (Exception ex)
            {
                log.LogError($"{nameof(ApprenticeshipEntity)} threw exception.", ex);
                throw;
            }
        }

        [FunctionName(nameof(PriceChangeApprovedEventServiceBusTrigger))]
        public async Task PriceChangeApprovedEventServiceBusTrigger(
            [NServiceBusTrigger(Endpoint = QueueNames.PriceChangeApproved)] PriceChangeApprovedEvent priceChangeApprovedEvent,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            log.LogInformation($"{nameof(PriceChangeApprovedEventServiceBusTrigger)} processing...");
            log.LogInformation("ApprenticeshipKey: {0} Received PriceChangeApprovedEvent: {1}",
                    priceChangeApprovedEvent.ApprenticeshipKey,
                    JsonSerializer.Serialize(priceChangeApprovedEvent, new JsonSerializerOptions { WriteIndented = true }));


            var entityId = new EntityId(nameof(ApprenticeshipEntity), priceChangeApprovedEvent.ApprenticeshipKey.ToString());
            await client.SignalEntityAsync(entityId, nameof(ApprenticeshipEntity.HandleApprenticeshipPriceChangeApprovedEvent), priceChangeApprovedEvent);
        }

        #region Temp
        [FunctionName(nameof(TriggerApproval))]
        public async Task TriggerApproval(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            log.LogInformation($"TriggerApproval: {requestBody}");
            var apprenticeshipCreatedEvent = JsonSerializer.Deserialize<ApprenticeshipCreatedEvent>(requestBody);
            await ApprenticeshipLearnerEventServiceBusTrigger(apprenticeshipCreatedEvent, client, log);
        }

        [FunctionName(nameof(TriggerRecalculate))]
        public async Task TriggerRecalculate(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            log.LogInformation($"TriggerRecalculate: {requestBody}");
            var priceChangeApprovedEvent = JsonSerializer.Deserialize<PriceChangeApprovedEvent>(requestBody);
            await PriceChangeApprovedEventServiceBusTrigger(priceChangeApprovedEvent, client, log);
        }
        #endregion
    }
}