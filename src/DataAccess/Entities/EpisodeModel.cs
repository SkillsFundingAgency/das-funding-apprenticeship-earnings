using SFA.DAS.Apprenticeships.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Dapper.Contrib.Extensions.Table("Domain.Episode")]
[Table("Episode", Schema = "Domain")]
public class EpisodeModel
{
    public EpisodeModel()
    {
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

    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
	public Guid ApprenticeshipKey { get; set; }
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
	public FundingType FundingType { get; set; }
	public long? FundingEmployerAccountId { get; set; }
    public string LegalEntityName { get; set; } = null!;
    public string TrainingCode { get; set; } = null!;
    public int AgeAtStartOfApprenticeship { get; set; }
    public List<EpisodePriceModel> Prices { get; set; } = new ();
    public EarningsProfileModel EarningsProfile { get; set; } = null!;
    public List<EarningsProfileHistoryModel> EarningsProfileHistory { get; set; } = new();
}