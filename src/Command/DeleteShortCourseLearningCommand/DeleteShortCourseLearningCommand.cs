namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.DeleteShortCourseLearningCommand;

public class DeleteShortCourseLearningCommand : ICommand
{
    public DeleteShortCourseLearningCommand(Guid learningKey)
    {
        LearningKey = learningKey;
    }

    public Guid LearningKey { get; }
}
