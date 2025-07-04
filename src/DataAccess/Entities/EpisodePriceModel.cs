using SFA.DAS.Learning.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Dapper.Contrib.Extensions.Table("Domain.EpisodePrice")]
[Table("EpisodePrice", Schema = "Domain")]
public class EpisodePriceModel
{
    public EpisodePriceModel()
    {
    }

    public EpisodePriceModel(Guid episodeKey, LearningEpisodePrice price)
    {
        Key = price.Key;
        EpisodeKey = episodeKey;
        StartDate = price.StartDate;
        EndDate = price.EndDate;
        AgreedPrice = price.TotalPrice;
        FundingBandMaximum = price.FundingBandMaximum;
    }

    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
	public Guid EpisodeKey { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
    public decimal AgreedPrice { get; set; }
	public decimal FundingBandMaximum { get; set; }
}