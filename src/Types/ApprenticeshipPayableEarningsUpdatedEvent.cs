namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

public class ApprenticeshipPayableEarningsUpdatedEvent
{
    public Guid LearningKey { get; set; }
    public Guid LearnerKey { get; set; }
    public string LearnerRef { get; set; } = string.Empty;
}
