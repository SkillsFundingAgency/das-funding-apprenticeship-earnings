namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

public class ShortCoursePayableEarningsUpdatedEvent
{
    public Guid LearningKey { get; set; }
    public Guid EpisodeKey { get; set; }
}
