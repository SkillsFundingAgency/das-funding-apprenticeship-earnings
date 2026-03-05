using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;

[Dapper.Contrib.Extensions.Table("Domain.ShortCourseEpisode")]
[Table("ShortCourseEpisode", Schema = "Domain")]
public class ShortCourseEpisodeEntity : BaseEpisodeEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal CoursePrice { get; set; }
    public ShortCourseEarningsProfileEntity EarningsProfile { get; set; }

    public ShortCourseEpisodeEntity()
    {
    }
}
