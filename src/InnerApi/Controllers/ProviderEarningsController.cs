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
        /// </remarks>
        [Route("fm36/{collectionYear}/{collectionPeriod}")]
        [HttpPost]
        public async Task<IActionResult> GetFm36Earnings([FromBody] List<Guid> learningKeys, long ukprn, short collectionYear, byte collectionPeriod)
        {
            var request = new GetFm36DataRequest(ukprn, collectionYear, collectionPeriod, learningKeys);
            var response = await queryDispatcher.Send<GetFm36DataRequest, GetFm36DataResponse>(request);
            return Ok(response);
        }
    }
}
