using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;

[Dapper.Contrib.Extensions.Table("Domain.ShortCourseEarningsProfile")]
[Table("ShortCourseEarningsProfile", Schema = "Domain")]
public class ShortCourseEarningsProfileEntity : BaseEarningsProfileEntity
{
    public List<ShortCourseInstalmentEntity> Instalments { get; set; } = null!;
        
}