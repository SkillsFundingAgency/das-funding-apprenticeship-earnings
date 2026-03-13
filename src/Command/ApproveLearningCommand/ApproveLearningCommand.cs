namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveLearningCommand;

public class ApproveLearningCommand : ICommand
{
    public ApproveLearningCommand(Guid learningKey)
    {
        LearningKey = learningKey;
    }

    public Guid LearningKey { get; }
}
