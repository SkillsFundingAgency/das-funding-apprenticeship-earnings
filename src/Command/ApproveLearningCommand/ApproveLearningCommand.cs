using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveLearningCommand;

public class ApproveLearningCommand : ICommand
{
    public ApproveLearningCommand(Guid learningKey, Guid episodeKey, long employerAccountId, long fundingAccountId, Guid learnerKey, string learnerRef, long approvalsApprenticeshipId, EmployerType employerType)
    {
        LearningKey = learningKey;
        EpisodeKey = episodeKey;
        EmployerAccountId = employerAccountId;
        FundingAccountId = fundingAccountId;
        LearnerKey = learnerKey;
        LearnerRef = learnerRef;
        ApprovalsApprenticeshipId = approvalsApprenticeshipId;
        EmployerType = employerType;
    }

    public Guid LearningKey { get; }
    public Guid EpisodeKey { get; }
    public long EmployerAccountId { get; }
    public long FundingAccountId { get; }
    public Guid LearnerKey { get; }
    public string LearnerRef { get; }
    public long ApprovalsApprenticeshipId { get; }
    public EmployerType EmployerType { get; }
}
