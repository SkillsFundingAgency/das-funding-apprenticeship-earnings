using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;

[Dapper.Contrib.Extensions.Table("History.EnglishAndMathsEarningsProfileHistory")]
[Table("EnglishAndMathsEarningsProfileHistory", Schema = "History")]
public class EnglishAndMathsEarningsProfileHistoryEntity : BaseEarningsProfileHistoryEntity
{
    [Required]
    public Guid EnglishAndMathsKey { get; set; }
}
