using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedShortCourseLearningCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateShortCourseOnProgrammeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourseEarnings;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers;

[ApiController]
[Route("shortCourses")]
public class ShortCoursesController : ControllerBase
{
    private readonly ILogger<ShortCoursesController> _logger;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public ShortCoursesController(
        ILogger<ShortCoursesController> logger,
        ICommandDispatcher commandDispatcher,
        IQueryDispatcher queryDispatcher)
    {
        _logger = logger;
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUnapprovedShortCourseLearning(
        CreateUnapprovedShortCourseLearningRequest request)
    {
        _logger.LogInformation(
            "Received request to create unapproved short course learning with key {LearningKey}",
            request?.LearningKey);

        try
        {
            var command = new CreateUnapprovedShortCourseLearningCommand(request);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating unapproved short course learning with key {LearningKey}",
                request?.LearningKey);

            return StatusCode(500);
        }

        _logger.LogInformation(
            "Successfully created unapproved short course learning with key {LearningKey}",
            request?.LearningKey);

        return Ok();
    }

    [HttpGet("/{learningKey}/shortCourses")]
    public async Task<IActionResult> GetShortCourseEarnings(Guid learningKey, [FromQuery] long ukprn)
    {
        _logger.LogInformation(
            "Received request to get short course earnings for LearningKey {LearningKey} and Ukprn {Ukprn}",
            learningKey, ukprn);

        try
        {
            var request = new GetShortCourseEarningsRequest(learningKey, ukprn);
            var response = await _queryDispatcher.Send<GetShortCourseEarningsRequest, GetShortCourseEarningsResponse>(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting short course earnings for LearningKey {LearningKey} and Ukprn {Ukprn}",
                learningKey, ukprn);

            return StatusCode(500);
        }
    }

    public async Task<IActionResult> UpdateOnProgramme(Guid learningKey, UpdateShortCourseOnProgrammeRequest request)
    {
        _logger.LogInformation("Received request to update ShortCourse on programme for LearningKey {LearningKey}", learningKey);

        try
        {
            var command = new UpdateShortCourseOnProgrammeCommand(learningKey, request);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ShortCourse on programme for LearningKey {LearningKey}", learningKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully updated ShortCourse on programme for LearningKey {LearningKey}", learningKey);
        return Ok();
    }
}