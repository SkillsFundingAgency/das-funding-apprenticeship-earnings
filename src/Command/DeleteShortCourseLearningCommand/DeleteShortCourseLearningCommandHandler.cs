using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.DeleteShortCourseLearningCommand;

public class DeleteShortCourseLearningCommandHandler : ICommandHandler<DeleteShortCourseLearningCommand>
{
    private readonly ILogger<DeleteShortCourseLearningCommandHandler> _logger;
    private readonly ILearningRepository _learningRepository;

    public DeleteShortCourseLearningCommandHandler(
        ILogger<DeleteShortCourseLearningCommandHandler> logger,
        ILearningRepository learningRepository)
    {
        _logger = logger;
        _learningRepository = learningRepository;
    }

    public async Task Handle(DeleteShortCourseLearningCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling DeleteShortCourseLearningCommand for LearningKey: {LearningKey}", command.LearningKey);

        var learning = await _learningRepository.GetShortCourseLearning(command.LearningKey);

        if (learning == null)
            throw new InvalidOperationException($"Short course learning not found for LearningKey {command.LearningKey}");

        var episode = learning.GetEpisode();

        episode.Delete();

        await _learningRepository.Update(learning);
    }
}
