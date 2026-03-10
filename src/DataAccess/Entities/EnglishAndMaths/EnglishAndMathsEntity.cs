using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;

[Dapper.Contrib.Extensions.Table("Domain.EnglishAndMaths")]
[Table("EnglishAndMaths", Schema = "Domain")]
public class EnglishAndMathsEntity
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid EarningsProfileId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Course { get; set; } = null!;
    public string LearnAimRef { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime? WithdrawalDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public int? PriorLearningAdjustmentPercentage { get; set; }

    public List<EnglishAndMathsInstalmentEntity> Instalments { get; set; } = [];
    public List<EnglishAndMathsPeriodInLearningEntity> PeriodsInLearning { get; set; } = [];

}