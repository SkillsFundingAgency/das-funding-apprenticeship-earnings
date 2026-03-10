using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;


public abstract class BaseLearningEntity
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string Uln { get; set; } = null!;
    public bool? HasEHCP { get; set; }
    public bool? IsCareLeaver { get; set; }
    public bool? CareLeaverEmployerConsentGiven { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public abstract class BaseLearningEntity<TEpisodeEntity> : BaseLearningEntity where TEpisodeEntity : BaseEpisodeEntity
{
    public List<TEpisodeEntity> Episodes { get; set; } = new();
}