namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveLearningCommand;

public class ApproveLearningCommand : ICommand
{
    public ApproveLearningCommand(Guid learningKey, Guid episodeKey)
    {
        LearningKey = learningKey;
        EpisodeKey = episodeKey;
    }

    public Guid LearningKey { get; }
    public Guid EpisodeKey { get; }
}
