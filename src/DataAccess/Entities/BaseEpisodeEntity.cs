using SFA.DAS.Learning.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public abstract class BaseEpisodeEntity
{
    public BaseEpisodeEntity()
    {
    }

    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
	public Guid LearningKey { get; set; }
    public long Ukprn { get; set; }
	public FundingType FundingType { get; set; }
    public string TrainingCode { get; set; } = null!;
    public DateTime? CompletionDate { get; set; }
    public DateTime? AchievementDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
}