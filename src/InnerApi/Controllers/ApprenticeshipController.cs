using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary;

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
        _logger.LogInformation("Received request to save care details for apprenticeship {apprenticeshipKey}", apprenticeshipKey);

        try
        {
            var command = new SaveCareDetailsCommand(apprenticeshipKey, saveCareDetailsRequest);
            await _commandDispatcher.Send(command);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving care details for apprenticeship {apprenticeshipKey}", apprenticeshipKey);
            return StatusCode(500);
        }

        _logger.LogInformation("Successfully saved care details for apprenticeship {apprenticeshipKey}", apprenticeshipKey);
        return Ok();
    }

}
