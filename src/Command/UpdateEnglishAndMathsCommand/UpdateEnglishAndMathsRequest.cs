using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;

public class UpdateEnglishAndMathsRequest
{
    public List<EnglishAndMathsItem> EnglishAndMaths { get; set; } = [];
}

public class EnglishAndMathsItem
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Course { get; set; } = null!;
    public string LearnAimRef { get; set; } = null!;
    public decimal Amount { get; set; }
    public int? PriorLearningAdjustmentPercentage { get; set; }
    public DateTime? PauseDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? ActualEndDate => CompletionDate ?? PauseDate ?? WithdrawalDate;
    public List<PeriodInLearningItem> PeriodsInLearning { get; set; } = [];
}
