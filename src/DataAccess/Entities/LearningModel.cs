using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Dapper.Contrib.Extensions.Table("Domain.Learning")]
[Table("Learning", Schema = "Domain")]
public class LearningModel
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
	public string Uln { get; set; } = null!;
    public List<EpisodeModel> Episodes { get; set; } = new();
    public bool? HasEHCP { get; set; }
    public bool? IsCareLeaver { get; set; }
    public bool? CareLeaverEmployerConsentGiven { get; set; }
    public DateTime DateOfBirth { get; set; }
}