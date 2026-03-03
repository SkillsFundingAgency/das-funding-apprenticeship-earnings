using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

[Dapper.Contrib.Extensions.Table("Domain.ApprenticeshipPeriodInLearning")]
[Table("ApprenticeshipPeriodInLearning", Schema = "Domain")]
public class ApprenticeshipPeriodInLearningEntity
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid EpisodeKey { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime OriginalExpectedEndDate { get; set; }
}