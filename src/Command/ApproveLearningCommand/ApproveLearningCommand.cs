namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveLearningCommand;

public class ApproveLearningCommand : ICommand
{
    public ApproveLearningCommand(Guid learningKey, long employerAccountId, long fundingAccountId)
    {
        LearningKey = learningKey;
        EmployerAccountId = employerAccountId;
        FundingAccountId = fundingAccountId;
    }

    public Guid LearningKey { get; }
    public long EmployerAccountId { get; }
    public long FundingAccountId { get; }
}
