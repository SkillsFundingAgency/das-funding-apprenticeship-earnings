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

    [Route("{apprenticeshipKey}/careDetails")]
    [HttpPatch]
    public async Task<IActionResult> SaveCareDetails(Guid apprenticeshipKey, SaveCareDetailsRequest saveCareDetailsRequest)
    {
        _logger.LogInformation("Received request to save care details for apprenticeship {ApprenticeshipKey}", apprenticeshipKey);

        try
        {
            var command = new SaveCareDetailsCommand(apprenticeshipKey, saveCareDetailsRequest);
            await _commandDispatcher.Send(command);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving care details for apprenticeship {ApprenticeshipKey}", apprenticeshipKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully saved care details for apprenticeship {ApprenticeshipKey}", apprenticeshipKey);
        return Ok();
    }

    [Route("{apprenticeshipKey}/learningSupport")]
    [HttpPatch]
    public async Task<IActionResult> SaveLearningSupport(Guid apprenticeshipKey, SaveLearningSupportRequest saveLearningSupportRequest)
    {
        _logger.LogInformation("Received request to save learning support for apprenticeship {ApprenticeshipKey}", apprenticeshipKey);

        try
        {
            var command = new SaveLearningSupportCommand(apprenticeshipKey, saveLearningSupportRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving learning support for apprenticeship {ApprenticeshipKey}", apprenticeshipKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully saved learning support for apprenticeship {ApprenticeshipKey}", apprenticeshipKey);
        return Ok();
    }

    [Route("{apprenticeshipKey}/mathsAndEnglish")]
    [HttpPatch]
    public async Task<IActionResult> SaveMathsAndEnglish(Guid apprenticeshipKey, SaveMathsAndEnglishRequest saveMathsAndEnglishRequest)
    {
        _logger.LogInformation("Received request to save maths and english for apprenticeship {ApprenticeshipKey}", apprenticeshipKey);

        try
        {
            var command = new SaveMathsAndEnglishCommand(apprenticeshipKey, saveMathsAndEnglishRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving maths and english for apprenticeship {ApprenticeshipKey}", apprenticeshipKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully saved maths and english for apprenticeship {ApprenticeshipKey}", apprenticeshipKey);
        return Ok();
    }
}
