﻿using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;

public class ProcessEpisodeUpdatedCommandHandler : ICommandHandler<ProcessEpisodeUpdatedCommand>
{
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;
    private readonly ISystemClockService _systemClock;

    public ProcessEpisodeUpdatedCommandHandler(
        IApprenticeshipRepository apprenticeshipRepository, 
        IMessageSession messageSession, 
        IApprenticeshipEarningsRecalculatedEventBuilder eventBuilder, 
        ISystemClockService systemClock)
    {
        _apprenticeshipRepository = apprenticeshipRepository;
        _messageSession = messageSession;
        _eventBuilder = eventBuilder;
        _systemClock = systemClock;
    }

    public async Task Handle(ProcessEpisodeUpdatedCommand command, CancellationToken cancellationToken = default)
    {
        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.EpisodeUpdatedEvent.LearningKey);

        apprenticeshipDomainModel.RecalculateEarnings(command.EpisodeUpdatedEvent, _systemClock);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

        await _messageSession.Publish(_eventBuilder.Build(apprenticeshipDomainModel));

    }
}