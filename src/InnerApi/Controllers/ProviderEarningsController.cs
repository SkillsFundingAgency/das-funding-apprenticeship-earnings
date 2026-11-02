using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers
{
    [ApiController]
    [Route("{ukprn}")]
    public class ProviderEarningsController(IQueryDispatcher queryDispatcher) : ControllerBase
    {
        /// <summary>
        /// Gets the fm36 earnings for the provider
        /// </summary>
        /// <remarks>
        /// Although this endpoint is a POST, it is used to retrieve data.
        /// Only earnings for learnings on the DAS funding platform (providers opted in to the new earnings calculation) are returned. If page and pageSize are supplied the results are paged.
        /// </remarks>
        [Route("fm36/{collectionYear}/{collectionPeriod}")]
        [HttpPost]
        public async Task<IActionResult> GetFm36Earnings([FromBody] List<Guid> learningKeys, long ukprn, short collectionYear, byte collectionPeriod, [FromQuery] int? page = null, [FromQuery] int? pageSize = null)
        {
            var request = new GetFm36DataRequest(ukprn, collectionYear, collectionPeriod, learningKeys, page, pageSize);
            var response = await queryDispatcher.Send<GetFm36DataRequest, GetFm36DataResponse>(request);
            return Ok(response);
        }
    }
}
