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

    public EpisodeModel(Guid learningKey, LearningEpisode learningEpisode, int fundingBandMaximum, DateTime? completionDate) : base()
    {
        Key = learningEpisode.Key;
        LearningKey = learningKey;
        Ukprn = learningEpisode.Ukprn;
        EmployerAccountId = learningEpisode.EmployerAccountId;
        FundingType = (FundingType)learningEpisode.FundingType;
        FundingEmployerAccountId = learningEpisode.FundingEmployerAccountId;
        LegalEntityName = learningEpisode.LegalEntityName;
        TrainingCode = learningEpisode.TrainingCode;
        FundingBandMaximum = fundingBandMaximum;
        CompletionDate = completionDate;

        var episodePrice = new EpisodePriceModel(Key, learningEpisode.Prices.First());

        Prices.Add(episodePrice);
        PeriodsInLearning.Add(episodePrice.ToSinglePeriodInLearning());
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
    public DateTime? CompletionDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public decimal FundingBandMaximum { get; set; }
    public List<EpisodePriceModel> Prices { get; set; } = new ();
    public List<EpisodePeriodInLearningModel> PeriodsInLearning { get; set; } = new ();
    public EarningsProfileModel EarningsProfile { get; set; } = null!;
    public TrainingType TrainingType { get; set; }
}

public enum TrainingType
{
    Apprenticeship = 1,
    ShortCourse = 2
}