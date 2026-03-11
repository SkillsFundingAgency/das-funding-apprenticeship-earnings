using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveLearningCommand;

public class ApproveLearningCommandHandler : ICommandHandler<ApproveLearningCommand>
{
    private readonly ILearningDomainService _learningDomainService;

    public ApproveLearningCommandHandler(ILearningDomainService learningDomainService)
    {
        _learningDomainService = learningDomainService;
    }

    public async Task Handle(ApproveLearningCommand command, CancellationToken cancellationToken = default)
    {
        var learning = await _learningDomainService.GetLearning(command.LearningKey)
            ?? throw new InvalidOperationException($"Learning not found for key: {command.LearningKey}");

        var episode = learning.GetFirstEpisode();
        episode.Approve();

        await _learningDomainService.Update(learning);
    }
}
