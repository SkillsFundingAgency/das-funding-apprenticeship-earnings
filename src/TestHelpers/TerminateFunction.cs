﻿using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

public static class TerminateFunction
{
    [FunctionName(nameof(TerminateFunction))]
    public static async Task Run([DurableClient] IDurableOrchestrationClient client)
    {
        var all = await client.ListInstancesAsync(new OrchestrationStatusQueryCondition
        {
            RuntimeStatus = new[] { OrchestrationRuntimeStatus.Pending, OrchestrationRuntimeStatus.Running, OrchestrationRuntimeStatus.ContinuedAsNew }
        }, CancellationToken.None);

        await Task.WhenAll(all.DurableOrchestrationState.Select(async o => await client.TerminateAsync(o.InstanceId, "Clean up test data.")));
    }
}