using System.Text.Json;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedShortCourseLearningCommand;

public class CreateUnapprovedShortCourseLearningCommandHandler
    : ICommandHandler<CreateUnapprovedShortCourseLearningCommand>
{
    private readonly ILogger<CreateUnapprovedShortCourseLearningCommandHandler> _logger;
    private readonly ISystemClockService _systemClockService;
    private IApprenticeshipFactory _apprenticeshipFactory;
    private IApprenticeshipRepository _apprenticeshipRepository;

    public CreateUnapprovedShortCourseLearningCommandHandler(
        ILogger<CreateUnapprovedShortCourseLearningCommandHandler> logger,
        ISystemClockService systemClockService, IApprenticeshipFactory apprenticeshipFactory, IApprenticeshipRepository apprenticeshipRepository)
    {
        _logger = logger;
        _systemClockService = systemClockService;
        _apprenticeshipFactory = apprenticeshipFactory;
        _apprenticeshipRepository = apprenticeshipRepository;
    }

    public async Task Handle(
        CreateUnapprovedShortCourseLearningCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling CreateUnapprovedShortCourseLearningCommand for learning {LearningKey}",
            command.Request.LearningKey);

        var existingShortCourse = await _apprenticeshipRepository.Get(command.Request.LearningKey);

        if (existingShortCourse != null)
        {
            existingShortCourse.UpdateDateOfBirth(command.Request.Learner.DateOfBirth);
            existingShortCourse.UpdateUnapprovedShortCourseInformation(new ShortCourseUpdateModel
            {
                CompletionDate = command.Request.OnProgramme.CompletionDate,
                CourseCode = command.Request.OnProgramme.CourseCode,
                EmployerId = command.Request.OnProgramme.EmployerId,
                ExpectedEndDate = command.Request.OnProgramme.ExpectedEndDate,
                StartDate = command.Request.OnProgramme.StartDate,
                TotalPrice = command.Request.OnProgramme.TotalPrice,
                Ukprn = command.Request.OnProgramme.Ukprn,
                Uln = command.Request.Learner.Uln,
                WithdrawalDate = command.Request.OnProgramme.WithdrawalDate
            });

            existingShortCourse.ApprenticeshipEpisodes.Single().CalculateShortCourseOnProgram(existingShortCourse, _systemClockService, false, JsonSerializer.Serialize(command.Request));

            await _apprenticeshipRepository.Update(existingShortCourse);
        }
        else
        {
            var shortCourse = _apprenticeshipFactory.CreateNewShortCourse(command.Request);

            shortCourse.ApprenticeshipEpisodes.Single().CalculateShortCourseOnProgram(shortCourse, _systemClockService, false, JsonSerializer.Serialize(command.Request));

            await _apprenticeshipRepository.Add(shortCourse);
        }

        _logger.LogInformation(
            "Successfully handled CreateUnapprovedShortCourseLearningCommand for learning {LearningKey}",
            command.Request.LearningKey);
    }
}