using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;

public class ProcessEpisodeUpdatedCommand : ICommand
{
    public ProcessEpisodeUpdatedCommand(ApprenticeshipEvent episodeUpdatedEvent)
    {
        EpisodeUpdatedEvent = episodeUpdatedEvent;
    }

    public ApprenticeshipEvent EpisodeUpdatedEvent { get; }

}