using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseRemoveCommand;

public class PauseRemoveCommandHandler : ICommandHandler<PauseRemoveCommand>
{

    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClock;

    public PauseRemoveCommandHandler(IApprenticeshipRepository apprenticeshipRepository, ISystemClockService systemClock)
    {
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(PauseRemoveCommand command, CancellationToken cancellationToken = default)
    {
        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeshipDomainModel.Pause(null, _systemClock);
        apprenticeshipDomainModel.Calculate(_systemClock);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

    }
}