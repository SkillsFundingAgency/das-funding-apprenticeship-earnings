using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Table("Domain.EpisodePrice")]
[System.ComponentModel.DataAnnotations.Schema.Table("Domain.EpisodePrice")]
public class EpisodePriceModel
{
    public EpisodePriceModel()
    {
    }

    public EpisodePriceModel(Guid episodeKey, ApprenticeshipEpisodePrice price)
    {
        Key = price.Key;
        EpisodeKey = episodeKey;
        StartDate = price.StartDate;
        EndDate = price.EndDate;
        AgreedPrice = price.TotalPrice;
        FundingBandMaximum = price.FundingBandMaximum;
    }

    [Key]
    public Guid Key { get; set; }
	public Guid EpisodeKey { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
    public decimal AgreedPrice { get; set; }
	public decimal FundingBandMaximum { get; set; }
}