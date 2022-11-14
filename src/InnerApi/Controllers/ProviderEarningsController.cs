using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers
{
    [Route("{ukprn}")]
    public class ProviderEarningsController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;

        public ProviderEarningsController(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [Route("summary")]
        [HttpGet]
        public async Task<IActionResult> Summary(string ukprn)
        {
            var request = new GetProviderEarningSummaryRequest(ukprn);
            var response = await _queryDispatcher.Send<GetProviderEarningSummaryRequest, GetProviderEarningSummaryResponse>(request);

            return Ok(response.ProviderEarningsSummary);
        }
    }
}
