using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;

#pragma warning disable CS8618

[Dapper.Contrib.Extensions.Table("Domain.ShortCourseEpisode")]
[Table("ShortCourseEpisode", Schema = "Domain")]
public class ShortCourseEpisodeEntity : BaseEpisodeEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal CoursePrice { get; set; }
    public MilestoneFlags Milestones { get; set; }
    public ShortCourseEarningsProfileEntity EarningsProfile { get; set; }

    public ShortCourseEpisodeEntity()
    {
    }
}

#pragma warning restore CS8618

[Flags]
public enum MilestoneFlags
{
    None = 0,
    ThirtyPercentLearningComplete = 1,
    LearningComplete = 2
}