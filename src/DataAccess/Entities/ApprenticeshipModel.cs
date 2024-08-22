namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Table("Domain.Apprenticeship")]
[System.ComponentModel.DataAnnotations.Schema.Table("Domain.Apprenticeship")]
public class ApprenticeshipModel
{
	public ApprenticeshipModel()
	{
        Episodes = new List<EpisodeModel>();
    }
        
	[Key]
	public Guid Key { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
	public string Uln { get; set; } = null!;
    public List<EpisodeModel> Episodes { get; set; }
}