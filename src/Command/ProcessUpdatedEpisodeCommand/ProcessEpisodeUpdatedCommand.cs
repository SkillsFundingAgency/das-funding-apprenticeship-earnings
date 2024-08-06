using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;

public class ProcessEpisodeUpdatedCommand
{
    public ProcessEpisodeUpdatedCommand(ApprenticeshipEntityModel apprenticeshipEntity, ApprenticeshipEvent episodeUpdatedEvent)
    {
        ApprenticeshipEntity = apprenticeshipEntity;
        EpisodeUpdatedEvent = episodeUpdatedEvent;
    }

    public ApprenticeshipEntityModel ApprenticeshipEntity { get; }
    public ApprenticeshipEvent EpisodeUpdatedEvent { get; }

}