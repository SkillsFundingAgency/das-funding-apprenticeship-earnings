using SFA.DAS.Learning.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Dapper.Contrib.Extensions.Table("Domain.Episode")]
[Table("Episode", Schema = "Domain")]
public class EpisodeModel
{
    public EpisodeModel()
    {
    }

    public EpisodeModel(Guid apprenticeshipKey, LearningEpisode learningEpisode) : base()
    {
        Key = learningEpisode.Key;
        ApprenticeshipKey = apprenticeshipKey;
        Ukprn = learningEpisode.Ukprn;
        EmployerAccountId = learningEpisode.EmployerAccountId;
        FundingType = (FundingType)learningEpisode.FundingType;
        FundingEmployerAccountId = learningEpisode.FundingEmployerAccountId;
        LegalEntityName = learningEpisode.LegalEntityName;
        TrainingCode = learningEpisode.TrainingCode;
        AgeAtStartOfApprenticeship = learningEpisode.AgeAtStartOfLearning;
        Prices.Add(new EpisodePriceModel(Key, learningEpisode.Prices.First()));
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