using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;

[Dapper.Contrib.Extensions.Table("Domain.ShortCourseLearning")]
[Table("ShortCourseLearning", Schema = "Domain")]
public class ShortCourseLearningEntity : BaseLearningEntity<ShortCourseEpisodeEntity>
{
}
