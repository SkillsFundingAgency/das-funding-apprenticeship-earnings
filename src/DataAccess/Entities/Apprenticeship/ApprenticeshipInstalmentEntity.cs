using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

[Dapper.Contrib.Extensions.Table("Domain.ApprenticeshipInstalment")]
[Table("ApprenticeshipInstalment", Schema = "Domain")]
public class ApprenticeshipInstalmentEntity : BaseInstalmentEntity
{
    public Guid EpisodePriceKey { get; set; }
}
