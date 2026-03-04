using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

[Dapper.Contrib.Extensions.Table("History.ApprenticeshipEarningsProfileHistory")]
[Table("ApprenticeshipEarningsProfileHistory", Schema = "History")]
public class ApprenticeshipEarningsProfileHistoryEntity : BaseEarningsProfileHistoryEntity
{

}