namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

public class ShortCoursePayableEarningsUpdatedEvent
{
    public Guid LearningKey { get; set; }
    public Guid EpisodeKey { get; set; }
    public long EmployerAccountId { get; set; }
    public long FundingAccountId { get; set; }
    public Guid LearnerKey { get; set; }
    public string LearnerRef { get; set; } = string.Empty;
}
