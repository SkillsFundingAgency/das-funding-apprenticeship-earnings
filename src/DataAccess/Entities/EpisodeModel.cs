using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Table("Domain.Episode")]
[System.ComponentModel.DataAnnotations.Schema.Table("Domain.Episode")]
public class EpisodeModel
{
    public EpisodeModel()
    {
        EarningsProfileHistory = new List<EarningsProfileHistoryModel>();
    }

    public EpisodeModel(Guid apprenticeshipKey, ApprenticeshipEpisode apprenticeshipEpisode) : base()
    {
        Key = apprenticeshipEpisode.Key;
        ApprenticeshipKey = apprenticeshipKey;
        Ukprn = apprenticeshipEpisode.Ukprn;
        EmployerAccountId = apprenticeshipEpisode.EmployerAccountId;
        FundingType = (FundingType)apprenticeshipEpisode.FundingType;
        FundingEmployerAccountId = apprenticeshipEpisode.FundingEmployerAccountId;
        LegalEntityName = apprenticeshipEpisode.LegalEntityName;
        TrainingCode = apprenticeshipEpisode.TrainingCode;
        AgeAtStartOfApprenticeship = apprenticeshipEpisode.AgeAtStartOfApprenticeship;
        Prices.Add(new EpisodePriceModel(Key, apprenticeshipEpisode.Prices.First()));
    }

    [Key]
    public Guid Key { get; set; }
	public Guid ApprenticeshipKey { get; set; }
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
	public FundingType FundingType { get; set; }
	public long? FundingEmployerAccountId { get; set; }
	public string LegalEntityName { get; set; }
    public string TrainingCode { get; set; } = null!;
    public int AgeAtStartOfApprenticeship { get; set; }
    public List<EpisodePriceModel> Prices { get; set; } = new ();
    public EarningsProfileModel EarningsProfile { get; set; } = null!;
    public List<EarningsProfileHistoryModel> EarningsProfileHistory { get; set; }
}