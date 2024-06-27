using Microsoft.Extensions.Internal;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApprovePriceChangeCommand;

public class ApprovePriceChangeCommandHandler : IApprovePriceChangeCommandHandler
{
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;
    private readonly ISystemClock _systemClock;

    public ApprovePriceChangeCommandHandler(
        IMessageSession messageSession, IApprenticeshipEarningsRecalculatedEventBuilder eventBuilder, ISystemClock systemClock)
    {
        _messageSession = messageSession;
        _eventBuilder = eventBuilder;
        _systemClock = systemClock;
    }

    public async Task<Apprenticeship> RecalculateEarnings(ApprovePriceChangeCommand command)
    {
        var apprenticeshipDomainModel = command.ApprenticeshipEntity.GetDomainModel();
        var agreedPrice = command.PriceChangeApprovedEvent.AssessmentPrice + command.PriceChangeApprovedEvent.TrainingPrice;
        apprenticeshipDomainModel.RecalculateEarnings(_systemClock, agreedPrice, command.PriceChangeApprovedEvent.EffectiveFromDate);
        await _messageSession.Publish(_eventBuilder.Build(apprenticeshipDomainModel));
        return apprenticeshipDomainModel;
    }
}
