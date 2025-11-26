using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseCommand;

public class PauseCommandHandler : ICommandHandler<PauseCommand>
{

    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClock;

    public PauseCommandHandler(IApprenticeshipRepository apprenticeshipRepository, ISystemClockService systemClock)
    {
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(PauseCommand command, CancellationToken cancellationToken = default)
    {
        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeshipDomainModel.Pause(command.PauseDate, _systemClock);
        apprenticeshipDomainModel.CalculateEarnings(_systemClock);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

    }
}
