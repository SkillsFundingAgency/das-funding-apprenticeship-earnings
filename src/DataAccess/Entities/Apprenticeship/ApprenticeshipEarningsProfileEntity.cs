using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

[Dapper.Contrib.Extensions.Table("Domain.ApprenticeshipEarningsProfile")]
[Table("ApprenticeshipEarningsProfile", Schema = "Domain")]
public class ApprenticeshipEarningsProfileEntity : BaseEarningsProfileEntity
{
    public List<ApprenticeshipInstalmentEntity> Instalments { get; set; } = null!;
    public List<ApprenticeshipAdditionalPaymentEntity> ApprenticeshipAdditionalPayments { get; set; } = null!;
    public List<EnglishAndMathsEntity> EnglishAndMathsCourses { get; set; } = null!;
}
