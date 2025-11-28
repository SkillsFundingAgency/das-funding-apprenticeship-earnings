namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.BreakInLearningCommand;

public class BreaksInLearningCommand : ICommand
{
    public Guid ApprenticeshipKey { get; }
    public Guid EpisodeKey { get; set; }
    public List<BreakInLearningPeriod> BreaksInLearning { get; set; }

    public BreaksInLearningCommand(Guid apprenticeshipKey, BreaksInLearningRequest breaksInLearningRequest)
    {
        ApprenticeshipKey = apprenticeshipKey;
        EpisodeKey = breaksInLearningRequest.EpisodeKey;
        BreaksInLearning = breaksInLearningRequest.BreaksInLearning;
    }
}
