using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;

public class ProcessWithdrawnApprenticeshipCommand : ICommand
{
    public ProcessWithdrawnApprenticeshipCommand(LearningWithdrawnEvent LearningWithdrawnEvent)
    {
        LearningKey = LearningWithdrawnEvent.LearningKey;
        ApprovalsApprenticeshipId = LearningWithdrawnEvent.ApprovalsApprenticeshipId;
        Reason = LearningWithdrawnEvent.Reason;
        LastDayOfLearning = LearningWithdrawnEvent.LastDayOfLearning;
    }

    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string Reason { get; set; }
    public DateTime LastDayOfLearning { get; set; }
}