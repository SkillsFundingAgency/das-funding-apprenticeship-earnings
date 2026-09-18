namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReleaseEarningsCommand;

public class ReleaseEarningsCommand : ICommand
{
    public Guid LearningKey { get; internal set; }
    public ReleaseEarningsRequest Request { get; set; }

    public ReleaseEarningsCommand(Guid learningKey, ReleaseEarningsRequest request)
    {
        LearningKey = learningKey;
        Request = request;
    }
}
