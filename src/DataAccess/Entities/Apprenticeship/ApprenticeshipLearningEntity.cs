using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

[Dapper.Contrib.Extensions.Table("Domain.ApprenticeshipLearning")]
[Table("ApprenticeshipLearning", Schema = "Domain")]
public class ApprenticeshipLearningEntity : BaseLearningEntity<ApprenticeshipEpisodeEntity>
{
}
