using DurableTask.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

public static class PurgeFunction
{
    [FunctionName(nameof(PurgeFunction))]
    public static async Task Run([DurableClient] IDurableOrchestrationClient client)
    {
        await client.PurgeInstanceHistoryAsync(
            DateTime.MinValue,
            null,
            new[] {
                OrchestrationStatus.Completed,
                OrchestrationStatus.Terminated,
                OrchestrationStatus.Failed,
            });
    }
}