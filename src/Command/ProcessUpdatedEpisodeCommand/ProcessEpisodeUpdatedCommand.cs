using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;

public class ProcessEpisodeUpdatedCommand(LearningEvent episodeUpdatedEvent) : ICommand
{
    public LearningEvent EpisodeUpdatedEvent { get; } = episodeUpdatedEvent;
}