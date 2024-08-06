using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;

public interface IProcessEpisodeUpdatedCommandHandler
{
    Task<Apprenticeship> RecalculateEarnings(ProcessEpisodeUpdatedCommand command);
}