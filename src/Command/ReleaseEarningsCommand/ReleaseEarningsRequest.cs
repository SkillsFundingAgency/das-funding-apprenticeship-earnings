namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReleaseEarningsCommand;

public class ReleaseEarningsRequest
{
    public Guid LearnerKey { get; set; }
    public string LearnerRef { get; set; } = string.Empty;
}
