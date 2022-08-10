using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.Threading.Tasks;
using SFA.DAS.Apprenticeships.Types;

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

                var entityId = new EntityId(nameof(ApprenticeshipEntity),
                    $"{Guid.NewGuid()} - {apprenticeshipCreatedEvent}");

                await client.SignalEntityAsync(entityId, nameof(ApprenticeshipEntity.HandleApprenticeshipLearnerEvent),
                    apprenticeshipCreatedEvent);

                log.LogInformation($"Started {nameof(ApprenticeshipEntity)} with EntityId = '{entityId}'.");
            }
            catch (Exception ex)
            {
                log.LogError($"{nameof(ApprenticeshipEntity)} threw exception.", ex);
                throw;
            }
        }
    }
}