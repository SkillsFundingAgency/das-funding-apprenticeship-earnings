using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;

public class UpdateOnProgrammeCommandHandler : ICommandHandler<UpdateOnProgrammeCommand>
{
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClock;

    public UpdateOnProgrammeCommandHandler(
        IApprenticeshipRepository apprenticeshipRepository,
        ISystemClockService systemClock)
    {
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(UpdateOnProgrammeCommand command, CancellationToken cancellationToken = default)
    {
        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeshipDomainModel.UpdateDateOfBirth(command.Request.DateOfBirth);

        if (command.Request.IncludesFundingBandMaximumUpdate)
        {
            apprenticeshipDomainModel.UpdatePrices(command.Request.Prices, command.Request.ApprenticeshipEpisodeKey, command.Request.FundingBandMaximum.Value, _systemClock);
        }

        

        apprenticeshipDomainModel.Calculate(_systemClock, command.Request.ApprenticeshipEpisodeKey);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

    }
}