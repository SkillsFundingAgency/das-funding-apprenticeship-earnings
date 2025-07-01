using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveLearningSupportCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveMathsAndEnglishCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers;

[ApiController]
[Route("apprenticeship")]
public class ApprenticeshipController: ControllerBase
{
    private readonly ILogger<ApprenticeshipController> _logger;
    private readonly ICommandDispatcher _commandDispatcher;

    public ApprenticeshipController(ILogger<ApprenticeshipController> logger, ICommandDispatcher commandDispatcher)
    {
        _logger = logger;
        _commandDispatcher = commandDispatcher;
    }

    [Route("{LearningKey}/careDetails")]
    [HttpPatch]
    public async Task<IActionResult> SaveCareDetails(Guid LearningKey, SaveCareDetailsRequest saveCareDetailsRequest)
    {
        _logger.LogInformation("Received request to save care details for apprenticeship {LearningKey}", LearningKey);

        try
        {
            var command = new SaveCareDetailsCommand(LearningKey, saveCareDetailsRequest);
            await _commandDispatcher.Send(command);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving care details for apprenticeship {LearningKey}", LearningKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully saved care details for apprenticeship {LearningKey}", LearningKey);
        return Ok();
    }

    [Route("{LearningKey}/learningSupport")]
    [HttpPatch]
    public async Task<IActionResult> SaveLearningSupport(Guid LearningKey, SaveLearningSupportRequest saveLearningSupportRequest)
    {
        _logger.LogInformation("Received request to save learning support for apprenticeship {LearningKey}", LearningKey);

        try
        {
            var command = new SaveLearningSupportCommand(LearningKey, saveLearningSupportRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving learning support for apprenticeship {LearningKey}", LearningKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully saved learning support for apprenticeship {LearningKey}", LearningKey);
        return Ok();
    }

    [Route("{LearningKey}/mathsAndEnglish")]
    [HttpPatch]
    public async Task<IActionResult> SaveMathsAndEnglish(Guid LearningKey, SaveMathsAndEnglishRequest saveMathsAndEnglishRequest)
    {
        _logger.LogInformation("Received request to save maths and english for apprenticeship {LearningKey}", LearningKey);

        try
        {
            var command = new SaveMathsAndEnglishCommand(LearningKey, saveMathsAndEnglishRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving maths and english for apprenticeship {LearningKey}", LearningKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully saved maths and english for apprenticeship {LearningKey}", LearningKey);
        return Ok();
    }
}
