namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveLearningCommand;

public class ApproveLearningCommand : ICommand
{
    public ApproveLearningCommand(Guid learningKey, Guid episodeKey, long employerAccountId, long fundingAccountId)
    {
        LearningKey = learningKey;
        EpisodeKey = episodeKey;
        EmployerAccountId = employerAccountId;
        FundingAccountId = fundingAccountId;
    }

    public Guid LearningKey { get; }
    public Guid EpisodeKey { get; }
    public long EmployerAccountId { get; }
    public long FundingAccountId { get; }
}
