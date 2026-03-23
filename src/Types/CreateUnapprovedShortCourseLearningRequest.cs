namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

#pragma warning disable CS8618
public class CreateUnapprovedShortCourseLearningRequest
{
    public Guid LearningKey { get; set; }
    public Guid EpisodeKey { get; set; }
    public Learner Learner { get; set; }
    public List<LearningSupportItem> LearningSupport { get; set; }
    public OnProgramme OnProgramme { get; set; }
}

public class Learner
{
    public DateTime DateOfBirth { get; set; }
    public string Uln { get; set; }
}

public class OnProgramme
{
    public string CourseCode { get; set; } = null!;

    public long EmployerId { get; set; }

    public long Ukprn { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? WithdrawalDate { get; set; }

    public DateTime? CompletionDate { get; set; }

    public DateTime ExpectedEndDate { get; set; }

    public List<Milestone> Milestones { get; set; } = new();

    public decimal TotalPrice { get; set; }
}

public enum Milestone
{
    ThirtyPercentLearningComplete = 1,
    LearningComplete = 2,
}

public class LearningSupportItem
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
#pragma warning restore CS8618