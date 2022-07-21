using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

public interface IOrchestrationData
{
    DurableOrchestrationStatus Status { get; set; }
}