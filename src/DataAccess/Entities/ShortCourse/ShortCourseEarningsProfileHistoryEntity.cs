using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;

[Dapper.Contrib.Extensions.Table("History.ShortCourseEarningsProfileHistory")]
[Table("ShortCourseEarningsProfileHistory", Schema = "History")]
public class ShortCourseEarningsProfileHistoryEntity : BaseEarningsProfileHistoryEntity
{

}