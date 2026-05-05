namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveShortCourseLearningCommand;

public class RemoveShortCourseLearningCommand : ICommand
{
    public RemoveShortCourseLearningCommand(Guid learningKey)
    {
        LearningKey = learningKey;
    }

    public Guid LearningKey { get; }
}
