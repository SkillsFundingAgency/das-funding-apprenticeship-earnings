using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.InternalEvents;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities
{
    public class EarningsFunctions
    {
        [FunctionName(nameof(ApprenticeshipLearnerEventServiceBusTrigger))]
        public async Task ApprenticeshipLearnerEventServiceBusTrigger(
            [ServiceBusTrigger(QueueNames.ApprenticeshipLearners)]
            InternalApprenticeshipLearnerEvent apprenticeshipLearnerEvent,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            try
            {
                log.LogInformation($"{nameof(ApprenticeshipLearnerEventServiceBusTrigger)} processing...");

                var entityId = new EntityId(nameof(ApprenticeshipEntity),
                    $"{Guid.NewGuid()} - {apprenticeshipLearnerEvent}");

                await client.SignalEntityAsync(entityId, nameof(ApprenticeshipEntity.Process),
                    apprenticeshipLearnerEvent);

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