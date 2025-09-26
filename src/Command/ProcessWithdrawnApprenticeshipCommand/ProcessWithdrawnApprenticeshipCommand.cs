namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;

public class ProcessWithdrawnApprenticeshipCommand : ICommand
{
    public ProcessWithdrawnApprenticeshipCommand(Guid apprenticeshipKey, DateTime lastDayOfLearning)
    {
        ApprenticeshipKey = apprenticeshipKey;
        LastDayOfLearning = lastDayOfLearning;
    }

    public Guid ApprenticeshipKey { get; set; }
    public DateTime LastDayOfLearning { get; set; }
}