using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

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
        learningDomainModel!.Remove(_systemClock);

        await _learningRepository.Update(learningDomainModel);
    }
}
