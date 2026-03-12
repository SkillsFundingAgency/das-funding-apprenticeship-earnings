using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateShortCourseOnProgrammeCommand;

public class UpdateShortCourseOnProgrammeCommandHandler : ICommandHandler<UpdateShortCourseOnProgrammeCommand>
{
    private readonly ILogger<UpdateShortCourseOnProgrammeCommandHandler> _logger;
    private readonly ILearningRepository _learningRepository;

    public UpdateShortCourseOnProgrammeCommandHandler(
        ILogger<UpdateShortCourseOnProgrammeCommandHandler> logger,
        ILearningRepository learningRepository)
    {
        _logger = logger;
        _learningRepository = learningRepository;
    }

    public async Task Handle(UpdateShortCourseOnProgrammeCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateShortCourseOnProgrammeCommand for LearningKey: {LearningKey}", command.LearningKey);

        var learning = _learningRepository.GetShortCourseLearning(command.LearningKey);

        if (learning == null)
            throw new InvalidOperationException($"Short course learning not found for LearningKey {command.LearningKey}");



    }
}
