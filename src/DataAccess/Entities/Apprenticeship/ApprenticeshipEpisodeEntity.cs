using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

[Dapper.Contrib.Extensions.Table("Domain.ApprenticeshipEpisode")]
[Table("ApprenticeshipEpisode", Schema = "Domain")]
public class ApprenticeshipEpisodeEntity : BaseEpisodeEntity
{
    public string TrainingCode { get; set; } = null!;
    public long EmployerAccountId { get; set; }
    public EmployerType EmployerType { get; set; }
    public long? FundingEmployerAccountId { get; set; }
    public string LegalEntityName { get; set; } = null!;
    public ApprenticeshipEarningsProfileEntity EarningsProfile { get; set; }
    public List<ApprenticeshipEpisodePriceEntity> Prices { get; set; } = new();
    public List<ApprenticeshipPeriodInLearningEntity> PeriodsInLearning { get; set; } = new();
    public DateTime? PauseDate { get; set; }
    public decimal FundingBandMaximum { get; set; }
    public bool IsRemoved { get; set; }

    public ApprenticeshipEpisodeEntity()
    {
    }

    public ApprenticeshipEpisodeEntity(Guid learningKey, LearningEpisode learningEpisode, int fundingBandMaximum, DateTime? completionDate)
    {
        Key = learningEpisode.Key;
        LearningKey = learningKey;
        Ukprn = learningEpisode.Ukprn;
        EmployerAccountId = learningEpisode.EmployerAccountId;
        EmployerType = learningEpisode.EmployerType == Learning.Enums.EmployerType.Levy ? EmployerType.Levy : EmployerType.NonLevy;
        FundingEmployerAccountId = learningEpisode.FundingEmployerAccountId;
        LegalEntityName = learningEpisode.LegalEntityName;
        TrainingCode = learningEpisode.TrainingCode;
        FundingBandMaximum = fundingBandMaximum;
        CompletionDate = completionDate;

        var episodePrice = new ApprenticeshipEpisodePriceEntity(Key, learningEpisode.Prices.First());

        Prices.Add(episodePrice);
        PeriodsInLearning.Add(episodePrice.ToSinglePeriodInLearning());
    }
}
