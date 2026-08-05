using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

#pragma warning disable CS8618
public class CreateUnapprovedApprenticeshipLearningRequest
{
    public Guid LearningKey { get; set; }
    public Guid EpisodeKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public DraftApprenticeshipLearner Learner { get; set; }
    public DraftApprenticeshipOnProgramme OnProgramme { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public DateTime? AchievementDate { get; set; }
    public List<LearningEpisodePrice> Prices { get; set; } = [];
    public List<ApprenticeshipPeriodInLearningItem> PeriodsInLearning { get; set; } = [];
    public List<DraftEnglishAndMathsItem> EnglishAndMaths { get; set; } = [];
    public List<LearningSupportItem> LearningSupport { get; set; } = [];
}

public class DraftApprenticeshipLearner
{
    public DateTime DateOfBirth { get; set; }
    public string Uln { get; set; }
    public DraftCare Care { get; set; }
}

public class DraftApprenticeshipOnProgramme
{
    public string TrainingCode { get; set; }
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
    public long? FundingEmployerAccountId { get; set; }
    public string LegalEntityName { get; set; }
    public FundingType FundingType { get; set; }
    public int? FundingBandMaximum { get; set; }
}

public class ApprenticeshipPeriodInLearningItem
{
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime OriginalExpectedEndDate { get; set; }
}

public class DraftEnglishAndMathsItem
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Course { get; set; } = null!;
    public string LearnAimRef { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal? CombinedFundingAdjustmentPercentage { get; set; }
    public DateTime? PauseDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public List<ApprenticeshipPeriodInLearningItem> PeriodsInLearning { get; set; } = [];
}

public class DraftCare
{
    public bool HasEHCP { get; set; }
    public bool IsCareLeaver { get; set; }
    public bool CareLeaverEmployerConsentGiven { get; set; }
}
#pragma warning restore CS8618
