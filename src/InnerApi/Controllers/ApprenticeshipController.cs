using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseRemoveCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReverseWithdrawal;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCompletionCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveLearningSupportCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveMathsAndEnglishCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SavePricesCommand;

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
        _logger.LogInformation("Received request to save maths and english for apprenticeship {apprenticeshipKey}", apprenticeshipKey);

        try
        {
            var command = new SaveMathsAndEnglishCommand(apprenticeshipKey, saveMathsAndEnglishRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving maths and english for apprenticeship {apprenticeshipKey}", apprenticeshipKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully saved maths and english for apprenticeship {apprenticeshipKey}", apprenticeshipKey);
        return Ok();
    }

    [Route("{apprenticeshipKey}/completion")]
    [HttpPatch]
    public async Task<IActionResult> SaveCompletion(Guid apprenticeshipKey, SaveCompletionRequest saveCompletionRequest)
    {
        _logger.LogInformation("Received request to save completion for apprenticeship {ApprenticeshipKey}", apprenticeshipKey);

        try
        {
            var command = new SaveCompletionCommand(apprenticeshipKey, saveCompletionRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving completion for apprenticeship {ApprenticeshipKey}", apprenticeshipKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully saved completion for apprenticeship {ApprenticeshipKey}", apprenticeshipKey);
        return Ok();
    }

    [Route("{apprenticeshipKey}/prices")]
    [HttpPatch]
    public async Task<IActionResult> SavePrices(Guid apprenticeshipKey, SavePricesRequest savePricesRequest)
    {
        _logger.LogInformation("Received request to save prices for apprenticeship {apprenticeshipKey}", apprenticeshipKey);

        try
        {
            var command = new SavePricesCommand(apprenticeshipKey, savePricesRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving prices for apprenticeship {apprenticeshipKey}", apprenticeshipKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully saved prices for apprenticeship {apprenticeshipKey}", apprenticeshipKey);
        return Ok();
    }

    [Route("{apprenticeshipKey}/withdraw")]
    [HttpPatch]
    public async Task<IActionResult> WithdrawLearner(Guid apprenticeshipKey, WithdrawRequest withdrawRequest)
    {
        _logger.LogInformation("Received request to withdraw learner {apprenticeshipKey}", apprenticeshipKey);

        try
        {
            var command = new ProcessWithdrawnApprenticeshipCommand(apprenticeshipKey, withdrawRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing for learner {apprenticeshipKey}", apprenticeshipKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully withdrew learner {apprenticeshipKey}", apprenticeshipKey);
        return Ok();
    }

    [Route("{apprenticeshipKey}/reverse-withdrawal")]
    [HttpPatch]
    public async Task<IActionResult> ReverseWithdrawal(Guid apprenticeshipKey, WithdrawRequest withdrawRequest)
    {
        _logger.LogInformation("Received request to reverse withdrawal of learner {apprenticeshipKey}", apprenticeshipKey);

        try
        {
            var command = new ReverseWithdrawalCommand(apprenticeshipKey);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reversing withdrawal of learner {apprenticeshipKey}", apprenticeshipKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully reversed withdrawal of learner {apprenticeshipKey}", apprenticeshipKey);
        return Ok();
    }

    [Route("{apprenticeshipKey}/pause")]
    [HttpPatch]
    public async Task<IActionResult> Pause(Guid apprenticeshipKey, PauseRequest pauseRequest)
    {
        _logger.LogInformation("Received request to pause learner {apprenticeshipKey}", apprenticeshipKey);

        try
        {
            var command = new PauseCommand(apprenticeshipKey, pauseRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing learner {apprenticeshipKey}", apprenticeshipKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully paused learner {apprenticeshipKey}", apprenticeshipKey);
        return Ok();
    }


    [Route("{apprenticeshipKey}/pause")]
    [HttpDelete]
    public async Task<IActionResult> RemovePause(Guid apprenticeshipKey)
    {
        _logger.LogInformation("Received request to remove pause for learner {apprenticeshipKey}", apprenticeshipKey);

        try
        {
            var command = new PauseRemoveCommand(apprenticeshipKey);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing pause for learner {apprenticeshipKey}", apprenticeshipKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully removed pause for learner {apprenticeshipKey}", apprenticeshipKey);
        return Ok();
    }
}
