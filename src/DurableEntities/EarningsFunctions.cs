using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Azure.WebJobs.Extensions.Http;
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
        
        [FunctionName("IncentivePaymentOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/IncentivePaymentOrchestrator")] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            var apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
            {
                ActualStartDate = DateTime.Now,
                AgeAtStartOfApprenticeship = 21,
                AgreedPrice = 10000,
                ApprenticeshipKey = Guid.NewGuid(),
                ApprovalsApprenticeshipId = 3245465,
                DateOfBirth = DateTime.Now.AddYears(-21),
                EmployerAccountId = 4325,
                FundingBandMaximum = 40000,
                FundingEmployerAccountId = null,
                FundingType = FundingType.Levy,
                LegalEntityName = "Test=",
                PlannedEndDate = DateTime.Now.AddYears(2),
                TrainingCode = "ABC123",
                UKPRN = 43985743,
                Uln = "34754536"
            };
            var entityId = new EntityId(nameof(ApprenticeshipEntity), apprenticeshipCreatedEvent.ApprenticeshipKey.ToString());

            await client.SignalEntityAsync(entityId, nameof(ApprenticeshipEntity.HandleApprenticeshipLearnerEvent), apprenticeshipCreatedEvent);

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}