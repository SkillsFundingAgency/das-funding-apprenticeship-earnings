using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Acceptance;

public class OrchestrationData : IOrchestrationData
{
    public DurableOrchestrationStatus Status { get; set; }
}