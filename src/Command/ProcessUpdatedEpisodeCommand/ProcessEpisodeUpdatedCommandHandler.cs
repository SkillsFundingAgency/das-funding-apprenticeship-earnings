using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;

public class ProcessEpisodeUpdatedCommandHandler : IProcessEpisodeUpdatedCommandHandler
{
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;
    private readonly ISystemClockService _systemClock;

    public ProcessEpisodeUpdatedCommandHandler(
        IMessageSession messageSession, IApprenticeshipEarningsRecalculatedEventBuilder eventBuilder, ISystemClockService systemClock)
    {
        _messageSession = messageSession;
        _eventBuilder = eventBuilder;
        _systemClock = systemClock;
    }

    public async Task<Apprenticeship> RecalculateEarnings(ProcessEpisodeUpdatedCommand command)
    {
        var apprenticeshipDomainModel = command.ApprenticeshipEntity.GetDomainModel();

        apprenticeshipDomainModel.RecalculateEarnings(command.EpisodeUpdatedEvent, _systemClock);

        await _messageSession.Publish(_eventBuilder.Build(apprenticeshipDomainModel));

        return apprenticeshipDomainModel;
    }
}