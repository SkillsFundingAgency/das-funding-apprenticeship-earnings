using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawMathsAndEnglishCommand;

public class ProcessWithdrawnMathsAndEnglishCommandHandler : ICommandHandler<ProcessWithdrawnMathsAndEnglishCommand>
{
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClock;

    public ProcessWithdrawnMathsAndEnglishCommandHandler(
        IApprenticeshipRepository apprenticeshipRepository,
        ISystemClockService systemClock)
    {
        _apprenticeshipRepository = apprenticeshipRepository;
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
    }
}