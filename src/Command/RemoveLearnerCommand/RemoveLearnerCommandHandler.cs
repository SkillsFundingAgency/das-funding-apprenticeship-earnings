using System.Text.Json;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveLearnerCommand;

public class RemoveLearnerCommandHandler : ICommandHandler<RemoveLearnerCommand>
{
    private readonly ILearningRepository _learningRepository;
    private readonly ISystemClockService _systemClock;

    public RemoveLearnerCommandHandler(ILearningRepository learningRepository, ISystemClockService systemClock)
    {
        _learningRepository = learningRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(RemoveLearnerCommand command, CancellationToken cancellationToken = default)
    {
        var learningDomainModel = await _learningRepository.GetApprenticeshipLearning(command.LearningKey);
        var episode = learningDomainModel!.GetCurrentEpisode(_systemClock);
        var startDate = episode.Prices.Min(x => x.StartDate);
        episode.UpdateWithdrawalDate(startDate, _systemClock);
        episode.UpdateEnglishAndMaths([], _systemClock);
        episode.RemoveAdditionalEarnings(_systemClock);
        learningDomainModel!.Calculate(_systemClock, JsonSerializer.Serialize(command.LearningKey));
        episode.UpdatePeriodsInLearning([]);

        await _learningRepository.Update(learningDomainModel);
    }
}