using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SavePricesCommand;

public class SavePricesCommandHandler : ICommandHandler<SavePricesCommand>
{
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClock;

    public SavePricesCommandHandler(
        IApprenticeshipRepository apprenticeshipRepository,
        ISystemClockService systemClock)
    {
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(SavePricesCommand command, CancellationToken cancellationToken = default)
    {
        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeshipDomainModel.UpdatePrices(command.Prices, command.ApprenticeshipEpisodeKey, command.FundingBandMaximum, command.AgeAtStartOfLearning, _systemClock);
        apprenticeshipDomainModel.Calculate(_systemClock, command.ApprenticeshipEpisodeKey);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

    }
}