﻿using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

public class GetStatusFunction
{
    private readonly IOrchestrationData _orchestrationData;

    public GetStatusFunction(IOrchestrationData orchestrationData)
    {
        _orchestrationData = orchestrationData;
    }

    [FunctionName(nameof(GetStatusFunction))]
    public async Task Run([DurableClient] IDurableOrchestrationClient client, string instanceId)
    {
        _orchestrationData.Status = await client.GetStatusAsync(instanceId);
    }
}