using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetAcademicYearEarnings;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers
{
    [ApiController]
    [Route("{ukprn}")]
    public class ProviderEarningsController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private ILogger<ProviderEarningsController> _logger;

        public ProviderEarningsController(IQueryDispatcher queryDispatcher, ILogger<ProviderEarningsController> logger)
        {
            _queryDispatcher = queryDispatcher;
            _logger = logger;
        }

        [Route("summary")]
        [HttpGet]
        public async Task<IActionResult> Summary(long ukprn)
        {
            _logger.LogInformation("Getting provider earnings summary for ukprn: {ukprn}", ukprn);

            var request = new GetProviderEarningSummaryRequest(ukprn);
            var response = await _queryDispatcher.Send<GetProviderEarningSummaryRequest, GetProviderEarningSummaryResponse>(request);

            return Ok(response.ProviderEarningsSummary);
        }

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Detail(long ukprn)
        {
            var request = new GetAcademicYearEarningsRequest(ukprn);
            var response = await _queryDispatcher.Send<GetAcademicYearEarningsRequest, GetAcademicYearEarningsResponse>(request);

            return Ok(response.AcademicYearEarnings);
        }
    }
}
