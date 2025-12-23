namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveLearnerCommand;

public class RemoveLearnerCommand(Guid apprenticeshipKey) : ICommand
{
    public Guid ApprenticeshipKey { get; set; } = apprenticeshipKey;
}