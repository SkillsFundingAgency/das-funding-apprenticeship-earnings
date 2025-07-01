using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;

public class ProcessEpisodeUpdatedCommand : ICommand
{
    public ProcessEpisodeUpdatedCommand(LearningEvent episodeUpdatedEvent)
    {
        EpisodeUpdatedEvent = episodeUpdatedEvent;
    }

    public LearningEvent EpisodeUpdatedEvent { get; }

}