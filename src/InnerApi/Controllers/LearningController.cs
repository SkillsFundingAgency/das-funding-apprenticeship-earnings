using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawMathsAndEnglishCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveLearnerCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveLearningSupportCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers;

[ApiController]
[Route("learning")]
public class LearningController: ControllerBase
{
    private readonly ILogger<LearningController> _logger;
    private readonly ICommandDispatcher _commandDispatcher;

    public LearningController(ILogger<LearningController> logger, ICommandDispatcher commandDispatcher)
    {
        _logger = logger;
        _commandDispatcher = commandDispatcher;
    }

    [Route("{learningKey}/careDetails")]
    [HttpPatch]
    public async Task<IActionResult> SaveCareDetails(Guid learningKey, SaveCareDetailsRequest saveCareDetailsRequest)
    {
        _logger.LogInformation("Received request to save care details for apprenticeship {learningKey}", learningKey);

        try
        {
            var command = new SaveCareDetailsCommand(learningKey, saveCareDetailsRequest);
            await _commandDispatcher.Send(command);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving care details for apprenticeship {learningKey}", learningKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully saved care details for apprenticeship {learningKey}", learningKey);
        return Ok();
    }

    [Route("{learningKey}/learningSupport")]
    [HttpPatch]
    public async Task<IActionResult> SaveLearningSupport(Guid learningKey, SaveLearningSupportRequest saveLearningSupportRequest)
    {
        _logger.LogInformation("Received request to save learning support for apprenticeship {learningKey}", learningKey);

        try
        {
            var command = new SaveLearningSupportCommand(learningKey, saveLearningSupportRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving learning support for apprenticeship {learningKey}", learningKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully saved learning support for apprenticeship {learningKey}", learningKey);
        return Ok();
    }

    [Route("{learningKey}/english-and-maths")]
    [HttpPut]
    public async Task<IActionResult> SaveMathsAndEnglish(Guid learningKey, UpdateEnglishAndMathsRequest saveMathsAndEnglishRequest)
    {
        _logger.LogInformation("Received request to update english and maths for apprenticeship {learningKey}", learningKey);

        try
        {
            var command = new UpdateEnglishAndMathsCommand(learningKey, saveMathsAndEnglishRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating english and maths for apprenticeship {learningKey}", learningKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully updated english and maths for apprenticeship {learningKey}", learningKey);
        return Ok();
    }

    [Route("{learningKey}/on-programme")]
    [HttpPut]
    public async Task<IActionResult> UpdateOnProgramme(Guid learningKey, UpdateOnProgrammeRequest updateOnProgrammeRequest)
    {
        _logger.LogInformation("Received request to update on-programme for apprenticeship {learningKey}", learningKey);

        try
        {
            var command = new UpdateOnProgrammeCommand(learningKey, updateOnProgrammeRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating on-programme for apprenticeship {learningKey}", learningKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully updated on-programme for apprenticeship {learningKey}", learningKey);
        return Ok();
    }

    [Route("{learningKey}")]
    [HttpDelete]
    public async Task<IActionResult> RemoveLearner(Guid learningKey)
    {
        _logger.LogInformation("Received request to remove learner {learningKey}", learningKey);

        try
        {
            var command = new RemoveLearnerCommand(learningKey);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing learner {learningKey}", learningKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully removed learner {learningKey}", learningKey);
        return Ok();
    }

    [Route("{learningKey}/mathsAndEnglish/withdraw")]
    [HttpPatch]
    public async Task<IActionResult> WithdrawMathsAndEnglish(Guid learningKey, MathsAndEnglishWithdrawRequest withdrawRequest)
    {
        _logger.LogInformation("Received request to withdraw maths and english course {course} for {learningKey}", withdrawRequest.Course, learningKey);

        try
        {
            var command = new ProcessWithdrawnMathsAndEnglishCommand(learningKey, withdrawRequest);
            await _commandDispatcher.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing for maths and english course {course} for {learningKey}", withdrawRequest.Course, learningKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully withdrew maths and english course {course} for {learningKey}", withdrawRequest.Course, learningKey);
        return Ok();
    }
}
