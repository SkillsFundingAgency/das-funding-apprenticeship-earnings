using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

public class UpdateEnglishAndMathsModel
{
    public TrackedValue<DateTime> StartDate { get; set; } = new TrackedValue<DateTime>(DateTime.MinValue);

    public TrackedValue<DateTime> EndDate { get; set; } = new TrackedValue<DateTime>(DateTime.MinValue);

    public TrackedValue<string> Course { get; set; } = new TrackedValue<string>(string.Empty);

    public TrackedValue<string> LearnAimRef { get; set; } = new TrackedValue<string>(string.Empty);

    public TrackedValue<decimal> Amount { get; set; } = new TrackedValue<decimal>(0);

    public TrackedValue<DateTime?> WithdrawalDate { get; set; } = new TrackedValue<DateTime?>(null);

    public TrackedValue<int?> PriorLearningAdjustmentPercentage { get; set; } = new TrackedValue<int?>(null);

    public TrackedValue<DateTime?> CompletionDate { get; set; } = new TrackedValue<DateTime?>(null);

    public TrackedValue<DateTime?> PauseDate { get; set; } = new TrackedValue<DateTime?>(null);

    public TrackedValue<List<PeriodInLearningItem>> PeriodsInLearning { get; set; } = new TrackedValue<List<PeriodInLearningItem>>(new List<PeriodInLearningItem>());
}