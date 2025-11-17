namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseCommand;

public class PauseCommand : ICommand
{
    public Guid ApprenticeshipKey { get; }
    public DateTime PauseDate { get; }

    public PauseCommand(Guid apprenticeshipKey, PauseRequest pauseRequest)
    {
        ApprenticeshipKey = apprenticeshipKey;
        PauseDate = pauseRequest.PauseDate;
    }
}
