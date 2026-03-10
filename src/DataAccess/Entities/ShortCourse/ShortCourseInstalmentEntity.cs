using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;

[Dapper.Contrib.Extensions.Table("Domain.ShortCourseInstalment")]
[Table("ShortCourseInstalment", Schema = "Domain")]
public class ShortCourseInstalmentEntity : BaseInstalmentEntity
{

}