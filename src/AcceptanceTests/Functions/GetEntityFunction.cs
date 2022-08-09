using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Functions
{
    public class GetEntityFunction
    {
        private readonly IOrchestrationData _orchestrationData;

        public GetEntityFunction(IOrchestrationData orchestrationData)
        {
            _orchestrationData = orchestrationData;
        }

        [FunctionName(nameof(GetEntityFunction))]
        public async Task Run([DurableClient] IDurableEntityClient client, string entityType, string entityKey, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            var entityResponse = await client.ReadEntityStateAsync<ApprenticeshipEntity>(new EntityId(entityType, entityKey));
            while (!entityResponse.EntityExists && !cts.IsCancellationRequested)
            {
                await Task.Delay(100);
                entityResponse = await client.ReadEntityStateAsync<ApprenticeshipEntity>(new EntityId(entityType, entityKey));
            }
            _orchestrationData.Entity = entityResponse.EntityState;
        }
    }
}
