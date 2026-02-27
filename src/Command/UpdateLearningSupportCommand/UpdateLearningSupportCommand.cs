using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand;

public class UpdateLearningSupportCommand : ICommand
{
    public UpdateLearningSupportCommand(Guid apprenticeshipKey, UpdateLearningSupportRequest updateLearningSupportRequest)
    {
        ApprenticeshipKey = apprenticeshipKey;
        LearningSupportPayments = updateLearningSupportRequest.LearningSupport;
    }

    public Guid ApprenticeshipKey { get; }
    public List<LearningSupportItem> LearningSupportPayments { get; set; } = new List<LearningSupportItem>();
}
