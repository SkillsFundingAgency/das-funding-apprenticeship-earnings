using Microsoft.Extensions.Internal;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;

public class ApproveStartDateChangeCommandHandler : IApproveStartDateChangeCommandHandler
{
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;
    private readonly ISystemClockService _systemClock;

    public ApproveStartDateChangeCommandHandler(
        IMessageSession messageSession, IApprenticeshipEarningsRecalculatedEventBuilder eventBuilder, ISystemClockService systemClock)
    {
        _messageSession = messageSession;
        _eventBuilder = eventBuilder;
        _systemClock = systemClock;
    }

    public async Task<Apprenticeship> RecalculateEarnings(ApproveStartDateChangeCommand command)
    {
        var apprenticeshipDomainModel = command.ApprenticeshipEntity.GetDomainModel();
        var newStartDate = command.ApprenticeshipStartDateChangedEvent.ActualStartDate;
        var newAgeAtStartOfApprenticeship = command.ApprenticeshipStartDateChangedEvent.AgeAtStartOfApprenticeship.GetValueOrDefault();
        var newPlannedEndDate = command.ApprenticeshipStartDateChangedEvent.PlannedEndDate;
        apprenticeshipDomainModel.RecalculateEarnings(_systemClock, newStartDate, newPlannedEndDate, newAgeAtStartOfApprenticeship);
        
        await _messageSession.Publish(_eventBuilder.Build(apprenticeshipDomainModel));
        
        return apprenticeshipDomainModel;
    }
}
