using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveLearnerCommand;

public class RemoveLearnerCommandHandler : ICommandHandler<RemoveLearnerCommand>
{
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClock;

    public RemoveLearnerCommandHandler(IApprenticeshipRepository apprenticeshipRepository, ISystemClockService systemClock)
    {
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(RemoveLearnerCommand command, CancellationToken cancellationToken = default)
    {
        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);
        var episode = apprenticeshipDomainModel.GetCurrentEpisode(_systemClock);
        var startDate = episode.Prices.Min(x => x.StartDate);
        episode.UpdateWithdrawalDate(startDate, _systemClock);
        episode.UpdateEnglishAndMaths([], _systemClock);
        episode.RemoveAdditionalEarnings(_systemClock);
        //episode.UpdateBreaksInLearning([]);
        apprenticeshipDomainModel.Calculate(_systemClock);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);
    }
}