using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;

public class ProcessWithdrawnApprenticeshipCommand(LearningWithdrawnEvent learningWithdrawnEvent) : ICommand
{
    public Guid ApprenticeshipKey { get; set; } = learningWithdrawnEvent.LearningKey;
    public long ApprovalsApprenticeshipId { get; set; } = learningWithdrawnEvent.ApprovalsApprenticeshipId;
    public string Reason { get; set; } = learningWithdrawnEvent.Reason;
    public DateTime LastDayOfLearning { get; set; } = learningWithdrawnEvent.LastDayOfLearning;
}