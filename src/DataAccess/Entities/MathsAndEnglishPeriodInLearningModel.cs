using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;


[Dapper.Contrib.Extensions.Table("Domain.MathsAndEnglishPeriodInLearning")]
[Table("MathsAndEnglishPeriodInLearning", Schema = "Domain")]
public class MathsAndEnglishPeriodInLearningModel
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid MathsAndEnglishKey { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime OriginalExpectedEndDate { get; set; }
}