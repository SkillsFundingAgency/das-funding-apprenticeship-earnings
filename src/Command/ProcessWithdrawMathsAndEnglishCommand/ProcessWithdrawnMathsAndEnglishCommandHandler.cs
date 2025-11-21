using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawMathsAndEnglishCommand;

public class ProcessWithdrawnMathsAndEnglishCommandHandler : ICommandHandler<ProcessWithdrawnMathsAndEnglishCommand>
{
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _eventBuilder;
    private readonly ISystemClockService _systemClock;

    public ProcessWithdrawnMathsAndEnglishCommandHandler(
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

    public async Task Handle(ProcessWithdrawnMathsAndEnglishCommand command, CancellationToken cancellationToken = default)
    {
        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeshipDomainModel.WithdrawMathsAndEnglishCourse(
            command.Course,
            command.WithdrawalDate,
            _systemClock);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

        await _messageSession.Publish(_eventBuilder.Build(apprenticeshipDomainModel));
    }
}