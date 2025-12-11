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
    public TrackedValue<List<BreakInLearningItem>> BreaksInLearning { get; set; } = new TrackedValue<List<BreakInLearningItem>>(new List<BreakInLearningItem>());
}