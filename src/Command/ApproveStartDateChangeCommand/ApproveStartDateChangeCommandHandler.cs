using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;

public class ApproveStartDateChangeCommandHandler : IApproveStartDateChangeCommandHandler
{
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;

    public ApproveStartDateChangeCommandHandler(IMessageSession messageSession, IApprenticeshipEarningsRecalculatedEventBuilder eventBuilder)
    {
        _messageSession = messageSession;
        _eventBuilder = eventBuilder;
    }

    public async Task<Apprenticeship> RecalculateEarnings(ApproveStartDateChangeCommand command)
    {
        var apprenticeshipDomainModel = command.ApprenticeshipEntity.GetDomainModel();
        var newStartDate = command.ApprenticeshipStartDateChangedEvent.ActualStartDate;
        var newAgeAtStartOfApprenticeship = command.ApprenticeshipStartDateChangedEvent.AgeAtStartOfApprenticeship.GetValueOrDefault();
        var newPlannedEndDate = command.ApprenticeshipStartDateChangedEvent.PlannedEndDate;
        apprenticeshipDomainModel.RecalculateEarnings(newStartDate, newPlannedEndDate, newAgeAtStartOfApprenticeship);
        
        await _messageSession.Publish(_eventBuilder.Build(apprenticeshipDomainModel));
        
        return apprenticeshipDomainModel;
    }
}
