using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApprovePriceChangeCommand;

public class ApprovePriceChangeCommandHandler : IApprovePriceChangeCommandHandler
{
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;

    public ApprovePriceChangeCommandHandler(IMessageSession messageSession, IApprenticeshipEarningsRecalculatedEventBuilder eventBuilder)
    {
        _messageSession = messageSession;
        _eventBuilder = eventBuilder;
    }

    public async Task<Apprenticeship> RecalculateEarnings(ApprovePriceChangeCommand command)
    {
        var apprenticeshipDomainModel = command.ApprenticeshipEntity.GetDomainModel();
        var agreedPrice = command.PriceChangeApprovedEvent.AssessmentPrice + command.PriceChangeApprovedEvent.TrainingPrice;
        apprenticeshipDomainModel.RecalculateEarnings(agreedPrice, command.PriceChangeApprovedEvent.EffectiveFromDate);
        await _messageSession.Publish(_eventBuilder.Build(apprenticeshipDomainModel));
        return apprenticeshipDomainModel;
    }
}
