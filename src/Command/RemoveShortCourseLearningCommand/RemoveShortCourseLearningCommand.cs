namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveShortCourseLearningCommand;

public class RemoveShortCourseLearningCommand : ICommand
{
    public RemoveShortCourseLearningCommand(Guid learningKey, Guid episodeKey, Guid learnerKey, string learnerRef)
    {
        LearningKey = learningKey;
        EpisodeKey = episodeKey;
        LearnerKey = learnerKey;
        LearnerRef = learnerRef;
    }

    public Guid LearningKey { get; }
    public Guid EpisodeKey { get; }
    public Guid LearnerKey { get; }
    public string LearnerRef { get; }
}
