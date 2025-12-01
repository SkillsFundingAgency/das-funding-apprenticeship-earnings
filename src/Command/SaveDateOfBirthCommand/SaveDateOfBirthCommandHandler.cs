using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveDateOfBirthCommand;

public class SaveDateOfBirthCommandHandler : ICommandHandler<SaveDateOfBirthCommand>
{

    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClock;

    public SaveDateOfBirthCommandHandler(IApprenticeshipRepository apprenticeshipRepository, ISystemClockService systemClock)
    {
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(SaveDateOfBirthCommand command, CancellationToken cancellationToken = default)
    {
        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeshipDomainModel.UpdateDateOfBirth(command.DateOfBirth);
        apprenticeshipDomainModel.Calculate(_systemClock);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

    }
}