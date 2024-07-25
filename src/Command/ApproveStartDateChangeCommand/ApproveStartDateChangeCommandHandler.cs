using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;

public class ApproveStartDateChangeCommandHandler : IApproveStartDateChangeCommandHandler
{
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;
    private readonly ISystemClockService _systemClock;
    private readonly ILogger<ApproveStartDateChangeCommandHandler> _logger;

    public ApproveStartDateChangeCommandHandler(
        IMessageSession messageSession, IApprenticeshipEarningsRecalculatedEventBuilder eventBuilder, ISystemClockService systemClock, ILogger<ApproveStartDateChangeCommandHandler> logger)
    {
        _messageSession = messageSession;
        _eventBuilder = eventBuilder;
        _systemClock = systemClock;
        _logger = logger;
    }

    public async Task<Apprenticeship> RecalculateEarnings(ApproveStartDateChangeCommand command)
    {
        var apprenticeshipDomainModel = command.ApprenticeshipEntity.GetDomainModel();
        var newStartDate = command.ApprenticeshipStartDateChangedEvent.ActualStartDate;
        var newAgeAtStartOfApprenticeship = command.ApprenticeshipStartDateChangedEvent.AgeAtStartOfApprenticeship.GetValueOrDefault();
        var newPlannedEndDate = command.ApprenticeshipStartDateChangedEvent.PlannedEndDate;
        var deletedPriceKeys = command.ApprenticeshipStartDateChangedEvent.DeletedPriceKeys;
        var changingPriceKey = command.ApprenticeshipStartDateChangedEvent.PriceKey;

        _logger.LogInformation(apprenticeshipDomainModel.Log107Data(_systemClock, deletedPriceKeys));
        apprenticeshipDomainModel.RecalculateEarningsStartDateChange(_systemClock, newStartDate, newPlannedEndDate, newAgeAtStartOfApprenticeship, deletedPriceKeys, changingPriceKey);
        
        await _messageSession.Publish(_eventBuilder.Build(apprenticeshipDomainModel));
        
        return apprenticeshipDomainModel;
    }
}
