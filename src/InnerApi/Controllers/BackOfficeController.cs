using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReReleaseEarningsGeneratedCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers;

[Route("[controller]")]
public class BackOfficeController : Controller
{
    ILogger<BackOfficeController> _logger;
    ICommandDispatcher _commandDispatcher;

    public BackOfficeController(ILogger<BackOfficeController> logger, ICommandDispatcher commandDispatcher)
    {
        _logger = logger;
        _commandDispatcher = commandDispatcher;
    }

    [Route("ReReleaseEarningsGenerated")]
    [HttpPost]
    public async Task<IActionResult> ReReleaseEarningsGenerated(long ukprn)
    {
        _logger.LogInformation("ReReleaseEarningsGenerated for Ukprn: {ukprn}", ukprn);

        try
        {
            await _commandDispatcher.Send(new ReReleaseEarningsGeneratedCommand(ukprn));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ReReleaseEarningsGenerated for Ukprn: {ukprn}", ukprn);
            return new StatusCodeResult(418);
        }

        return Ok();
    }
}
