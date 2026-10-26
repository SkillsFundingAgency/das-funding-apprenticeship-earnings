using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApplyFurtherApprovalCommand;

public class ApplyFurtherApprovalCommandHandler(
    ILearningRepository learningRepository,
    ILogger<ApplyFurtherApprovalCommandHandler> logger)
    : ICommandHandler<ApplyFurtherApprovalCommand>
{
    public async Task Handle(ApplyFurtherApprovalCommand command, CancellationToken cancellationToken = default)
    {
        var learning = await learningRepository.GetApprenticeshipLearning(command.LearningKey);

        if (learning == null || !learning.HasEpisode(command.EpisodeKey))
        {
            logger.LogWarning("No earnings found for LearningKey {LearningKey}, EpisodeKey {EpisodeKey}; further approval ignored",
                command.LearningKey, command.EpisodeKey);
            return;
        }

        learning.ApplyFurtherApproval(command.EpisodeKey, command.LearnerKey, command.LearnerRef);

        await learningRepository.Update(learning);
    }
}
