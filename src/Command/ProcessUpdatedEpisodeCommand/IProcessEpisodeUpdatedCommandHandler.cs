namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;

public interface IProcessEpisodeUpdatedCommandHandler
{
    Task RecalculateEarnings(ProcessEpisodeUpdatedCommand command);
}