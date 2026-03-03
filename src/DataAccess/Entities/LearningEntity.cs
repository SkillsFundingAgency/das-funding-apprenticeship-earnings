using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Dapper.Contrib.Extensions.Table("Domain.Learning")]
[Table("Learning", Schema = "Domain")]
public class LearningEntity
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
	public string Uln { get; set; } = null!;
    public List<ApprenticeshipEpisodeEntity> ApprenticeshipEpisodes { get; set; } = new();
    public List<ShortCourseEpisodeEntity> ShortCourseEpisodes { get; set; } = new();
    public bool? HasEHCP { get; set; }
    public bool? IsCareLeaver { get; set; }
    public bool? CareLeaverEmployerConsentGiven { get; set; }
    public DateTime DateOfBirth { get; set; }
}