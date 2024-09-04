using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetAcademicYearEarnings;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers
{
    [ApiController]
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
        public async Task<IActionResult> Summary(long ukprn)
        {
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

        [Route("fm36")]
        [HttpGet]
        public async Task<IActionResult> EarningsApprenticeships(long ukprn)
        {
            var request = new GetFm36DataRequest(ukprn);
            var response = await _queryDispatcher.Send<GetFm36DataRequest, GetFm36DataResponse>(request);

            if(response== null || !response.Any())
            {
                return NotFound();
            }

            return Ok(response);
        }
    }
}
