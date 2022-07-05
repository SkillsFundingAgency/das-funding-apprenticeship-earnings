//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.DurableTask;
//using Microsoft.Extensions.Logging;
//using SFA.DAS.NServiceBus.AzureFunction.Attributes;
//using System;
//using System.Threading.Tasks;
//using SFA.DAS.Funding.Events;

//namespace SFA.DAS.Funding.ApprenticeshipEarnings.Funcs60
//{
//    public class EarningsFunctions
//    {
//        [FunctionName(nameof(ApprenticeshipLearnerEventServiceBusTrigger))]
//        public async Task ApprenticeshipLearnerEventServiceBusTrigger(
//            [NServiceBusTrigger(Endpoint = QueueNames.Sandbox)] SampleFundingEvent @event,
//            [DurableClient] IDurableEntityClient client,
//            ILogger log)
//        {

//            try
//            {
//                log.LogInformation($"{nameof(ApprenticeshipLearnerEventServiceBusTrigger)} processing...");

//                var apprenticeshipLearnerData = @event.CorrelationId;

//                var entityId = new EntityId(nameof(ApprenticeshipEntity), $"{Guid.NewGuid()} - {apprenticeshipLearnerData}");

//                await client.SignalEntityAsync(entityId, nameof(ApprenticeshipEntity.Process), apprenticeshipLearnerData);


//                log.LogInformation($"Started {nameof(ApprenticeshipEntity)} with EntityId = '{entityId}'.");
//            }
//            catch (Exception ex)
//            {
//                log.LogError($"{nameof(ApprenticeshipEntity)} threw exception.", ex);
//                throw;
//            }
//        }
//    }
//}