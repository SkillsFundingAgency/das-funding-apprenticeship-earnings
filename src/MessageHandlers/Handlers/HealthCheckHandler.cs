using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.AppStart;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class HealthCheckHandler(FunctionHealthChecker functionHealthChecker)
{
    [Function(nameof(HealthCheck))]
    public async Task<IActionResult> HealthCheck([HttpTrigger(AuthorizationLevel.Function, "get", Route = "HealthCheck")] HttpRequestMessage req, CancellationToken cancellationToken)
    {
        var result = await functionHealthChecker.HealthCheck(cancellationToken);
        if (!result)
        {
            return new ObjectResult("Unhealthy"){ StatusCode = 503 };
        }

        return new OkObjectResult("Healthy");
    }
}
