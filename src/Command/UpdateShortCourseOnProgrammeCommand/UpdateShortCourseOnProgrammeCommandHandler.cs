using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using System.Text.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateShortCourseOnProgrammeCommand;

public class UpdateShortCourseOnProgrammeCommandHandler : ICommandHandler<UpdateShortCourseOnProgrammeCommand, UpdateShortCourseOnProgrammeResponse>
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

    public async Task<UpdateShortCourseOnProgrammeResponse> Handle(UpdateShortCourseOnProgrammeCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateShortCourseOnProgrammeCommand for LearningKey: {LearningKey}", command.LearningKey);

        var learning = await _learningRepository.GetShortCourseLearning(command.LearningKey);

        if (learning == null)
            throw new InvalidOperationException($"Short course learning not found for LearningKey {command.LearningKey}");

        learning.UpdateOnProgramme(
            command.EpisodeKey,
            command.Request.CompletionDate,
            command.Request.WithdrawalDate,
            command.Request.Milestones,
            JsonSerializer.Serialize(command.Request),
            command.Request.LearnerKey,
            command.Request.LearnerRef);
            //todo assert this results in two new values correctly on event

        await _learningRepository.Update(learning);

        return learning.ToDtoResponse<UpdateShortCourseOnProgrammeResponse>(command.EpisodeKey);
    }

}
