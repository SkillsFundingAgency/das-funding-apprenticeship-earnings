using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedShortCourseLearningCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers;

[ApiController]
[Route("shortCourses")]
public class ShortCoursesController : ControllerBase
{
    private readonly ILogger<ShortCoursesController> _logger;
    private readonly ICommandDispatcher _commandDispatcher;

    public ShortCoursesController(
        ILogger<ShortCoursesController> logger,
        ICommandDispatcher commandDispatcher)
    {
        _logger = logger;
        _commandDispatcher = commandDispatcher;
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
}