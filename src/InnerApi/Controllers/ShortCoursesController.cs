using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedShortCourseLearningCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveShortCourseLearningCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateShortCourseOnProgrammeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm99ShortCourseEarnings;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourse;
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

    [HttpGet("/fm99/{learningKey}/shortCourses/{episodeKey}")]
    public async Task<IActionResult> GetFm99ShortCourseEarnings(Guid learningKey, Guid episodeKey)
    {
        _logger.LogInformation(
            "Received request to get fm99 short course earnings for LearningKey {LearningKey} and EpisodeKey {EpisodeKey}",
            learningKey, episodeKey);

        try
        {
            var request = new GetFm99ShortCourseEarningsRequest(learningKey, episodeKey);
            var response = await _queryDispatcher.Send<GetFm99ShortCourseEarningsRequest, GetFm99ShortCourseEarningsResponse>(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting short course earnings for LearningKey {LearningKey} and EpisodeKey {EpisodeKey}",
                learningKey, episodeKey);

            return StatusCode(500);
        }
    }

    [HttpPut("/{learningKey}/shortCourses/{episodeKey}/on-programme")]
    public async Task<IActionResult> UpdateOnProgramme(Guid learningKey, Guid episodeKey, UpdateShortCourseOnProgrammeRequest request)
    {
        _logger.LogInformation("Received request to update ShortCourse on programme for LearningKey {LearningKey}", learningKey);

        UpdateShortCourseOnProgrammeResponse? response = null;

        try
        {
            var command = new UpdateShortCourseOnProgrammeCommand(learningKey, episodeKey, request);
            response = await _commandDispatcher.Send<UpdateShortCourseOnProgrammeCommand, UpdateShortCourseOnProgrammeResponse>(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ShortCourse on programme for LearningKey {LearningKey}", learningKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully updated ShortCourse on programme for LearningKey {LearningKey}", learningKey);
        return Ok(response);
    }

    [HttpGet("/{learningKey}/shortCourses/{episodeKey}")]
    public async Task<IActionResult> GetShortCourse(Guid learningKey, Guid episodeKey)
    {
        _logger.LogInformation(
            "Received request to get short course for LearningKey {LearningKey} and EpisodeKey {EpisodeKey}",
            learningKey, episodeKey);

        try
        {
            var request = new GetShortCourseRequest(learningKey, episodeKey);
            var response = await _queryDispatcher.Send<GetShortCourseRequest, GetShortCourseResponse>(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting short course for LearningKey {LearningKey} and EpisodeKey {EpisodeKey}",
                learningKey, episodeKey);

            return StatusCode(500);
        }
    }

    [HttpDelete("/{learningKey}/shortCourses/{episodeKey}")]
    public async Task<IActionResult> RemoveShortCourseLearning(Guid learningKey, Guid episodeKey)
    {
        _logger.LogInformation("Received request to remove ShortCourse learning for LearningKey {LearningKey}", learningKey);

        RemoveShortCourseLearningResponse? response = null;

        try
        {
            var command = new RemoveShortCourseLearningCommand(learningKey, episodeKey);
            response = await _commandDispatcher.Send<RemoveShortCourseLearningCommand, RemoveShortCourseLearningResponse>(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing ShortCourse learning for LearningKey {LearningKey}", learningKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully removed ShortCourse learning for LearningKey {LearningKey}", learningKey);
        return Ok(response);
    }
}
