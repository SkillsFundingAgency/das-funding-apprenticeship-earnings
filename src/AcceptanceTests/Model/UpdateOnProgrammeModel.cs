using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

public class UpdateOnProgrammeModel
{
    // Price changes
    public TrackedValue<DateTime> PriceStartDate { get; set; } = new TrackedValue<DateTime>(DateTime.MinValue);
    public TrackedValue<DateTime> PriceEndDate{ get; set; } = new TrackedValue<DateTime>(DateTime.MinValue);
    public TrackedValue<decimal> NewTrainingPrice { get; set; } = new TrackedValue<decimal>(0);
    public TrackedValue<decimal> NewAssessmentPrice { get; set; } = new TrackedValue<decimal>(0);

    // Date of birth change
    public TrackedValue<DateTime?> DateOfBirth { get; set; } = new TrackedValue<DateTime?>(null);

    // Pausing and Breaks
    public TrackedValue<DateTime?> PauseDate { get; set; } = new TrackedValue<DateTime?>(null);
    public TrackedValue<List<PeriodInLearningItem>> PeriodsInLearning { get; set; } = new TrackedValue<List<PeriodInLearningItem>>(new List<PeriodInLearningItem>());

    // Completion
    public TrackedValue<DateTime?> CompletionDate { get; set; } = new TrackedValue<DateTime?>(null);

    // Withdrawal
    public TrackedValue<DateTime?> WithdrawalDate { get; set; } = new TrackedValue<DateTime?>(null);

    // Care Details
    public TrackedValue<bool> HasEHCP { get; set; } = new TrackedValue<bool>(false);
    public TrackedValue<bool> IsCareLeaver { get; set; } = new TrackedValue<bool>(false);
    public TrackedValue<bool> CareLeaverEmployerConsentGiven { get; set; } = new TrackedValue<bool>(false);
}