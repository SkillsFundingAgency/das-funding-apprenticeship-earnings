using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.ProviderEarnings.InnerApi;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers
{
    //todo this can be thrown away, but for now proves data access
    [ApiController]
    [Route("[controller]")]
    public class DataAccessTestConnectionController : ControllerBase
    {
        private readonly ILogger<DataAccessTestConnectionController> _logger;
        private readonly ApprenticeshipEarningsDataContext _context;

        public DataAccessTestConnectionController(ILogger<DataAccessTestConnectionController> logger, ApprenticeshipEarningsDataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "TestDBConnection")]
        public IEnumerable<long> GetUkprns()
        {
            return _context.Earning.Select(x => x.Ukprn);
        }
    }
}