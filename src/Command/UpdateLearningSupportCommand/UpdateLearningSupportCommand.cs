using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand;

public class UpdateLearningSupportCommand : ICommand
{
    public UpdateLearningSupportCommand(Guid learningKey, UpdateLearningSupportRequest updateLearningSupportRequest)
    {
        LearningKey = learningKey;
        LearningSupportPayments = updateLearningSupportRequest.LearningSupport;
    }

    public Guid LearningKey { get; }
    public List<LearningSupportItem> LearningSupportPayments { get; set; } = new List<LearningSupportItem>();
}
