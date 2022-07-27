using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

public interface IOrchestrationData
{
    DurableOrchestrationStatus Status { get; set; }
}