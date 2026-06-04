namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveShortCourseLearningCommand;

public class RemoveShortCourseLearningCommand : ICommand
{
    public RemoveShortCourseLearningCommand(Guid learningKey, Guid episodeKey)
    {
        LearningKey = learningKey;
        EpisodeKey = episodeKey;
    }

    public Guid LearningKey { get; }
    public Guid EpisodeKey { get; }
}
