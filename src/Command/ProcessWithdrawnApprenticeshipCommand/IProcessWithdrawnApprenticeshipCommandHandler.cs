namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;

public interface IProcessWithdrawnApprenticeshipCommandHandler
{
    Task Process(ProcessWithdrawnApprenticeshipCommand command);
}