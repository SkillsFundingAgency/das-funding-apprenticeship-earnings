namespace SFA.DAS.Funding.ApprenticeshipEarnings.Events;

public class EarningsGeneratedEvent
{
    public string ApprenticeshipKey { get; set; }
    public long CommitmentId { get; set; }
}