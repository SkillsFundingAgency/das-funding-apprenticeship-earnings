using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveShortCourseLearningCommand;

public class RemoveShortCourseLearningCommandHandler(
    ILogger<RemoveShortCourseLearningCommandHandler> logger,
    ILearningRepository learningRepository)
    : ICommandHandler<RemoveShortCourseLearningCommand, RemoveShortCourseLearningResponse>
{
    public async Task<RemoveShortCourseLearningResponse> Handle(RemoveShortCourseLearningCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling RemoveShortCourseLearningCommand for LearningKey: {LearningKey}", command.LearningKey);

        var learning = await learningRepository.GetShortCourseLearning(command.LearningKey);

        if (learning == null)
            throw new InvalidOperationException($"Short course learning not found for LearningKey {command.LearningKey}");

        var episode = learning.GetEpisode();

        episode.Remove();

        await learningRepository.Update(learning);

        return learning.ToDtoResponse<RemoveShortCourseLearningResponse>();
    }
}
