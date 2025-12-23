namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand;

public class UpdateLearningSupportRequest
{
    public List<LearningSupportItem> LearningSupport { get; set; } = [];
}

public class LearningSupportItem
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
