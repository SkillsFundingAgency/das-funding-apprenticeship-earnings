namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveLearnerCommand;

public class RemoveLearnerCommand(Guid learningKey) : ICommand
{
    public Guid LearningKey { get; set; } = learningKey;
}