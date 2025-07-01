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

    public EpisodeModel(Guid LearningKey, LearningEpisode LearningEpisode) : base()
    {
        Key = LearningEpisode.Key;
        LearningKey = LearningKey;
        Ukprn = LearningEpisode.Ukprn;
        EmployerAccountId = LearningEpisode.EmployerAccountId;
        FundingType = (FundingType)LearningEpisode.FundingType;
        FundingEmployerAccountId = LearningEpisode.FundingEmployerAccountId;
        LegalEntityName = LearningEpisode.LegalEntityName;
        TrainingCode = LearningEpisode.TrainingCode;
        AgeAtStartOfLearning = LearningEpisode.AgeAtStartOfLearning;
        Prices.Add(new EpisodePriceModel(Key, LearningEpisode.Prices.First()));
    }

    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
	public Guid LearningKey { get; set; }
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
	public FundingType FundingType { get; set; }
	public long? FundingEmployerAccountId { get; set; }
    public string LegalEntityName { get; set; } = null!;
    public string TrainingCode { get; set; } = null!;
    public int AgeAtStartOfLearning { get; set; }
    public List<EpisodePriceModel> Prices { get; set; } = new ();
    public EarningsProfileModel EarningsProfile { get; set; } = null!;
    public List<EarningsProfileHistoryModel> EarningsProfileHistory { get; set; } = new();
}