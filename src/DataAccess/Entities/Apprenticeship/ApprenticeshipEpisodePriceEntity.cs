using SFA.DAS.Learning.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

[Dapper.Contrib.Extensions.Table("Domain.ApprenticeshipEpisodePrice")]
[Table("ApprenticeshipEpisodePrice", Schema = "Domain")]
public class ApprenticeshipEpisodePriceEntity
{
    public ApprenticeshipEpisodePriceEntity()
    {
    }

    public ApprenticeshipEpisodePriceEntity(Guid episodeKey, LearningEpisodePrice price)
    {
        Key = price.Key;
        EpisodeKey = episodeKey;
        StartDate = price.StartDate;
        EndDate = price.EndDate;
        AgreedPrice = price.TotalPrice;
    }

    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
	public Guid EpisodeKey { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
    public decimal AgreedPrice { get; set; }
}