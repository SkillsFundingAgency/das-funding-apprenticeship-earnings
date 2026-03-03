using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;


[Dapper.Contrib.Extensions.Table("Domain.EnglishAndMathsPeriodInLearning")]
[Table("EnglishAndMathsPeriodInLearning", Schema = "Domain")]
public class EnglishAndMathsPeriodInLearningEntity
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid EnglishAndMathsKey { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime OriginalExpectedEndDate { get; set; }
}