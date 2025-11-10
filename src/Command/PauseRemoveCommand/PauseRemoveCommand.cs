using SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseRemoveCommand;

public class PauseRemoveCommand : ICommand
{
    public Guid ApprenticeshipKey { get; }

    public PauseRemoveCommand(Guid apprenticeshipKey)
    {
        ApprenticeshipKey = apprenticeshipKey;
    }
}
