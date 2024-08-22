using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;

public class ProcessEpisodeUpdatedCommand
{
    public ProcessEpisodeUpdatedCommand(ApprenticeshipEvent episodeUpdatedEvent)
    {
        EpisodeUpdatedEvent = episodeUpdatedEvent;
    }

    public ApprenticeshipEvent EpisodeUpdatedEvent { get; }

}