using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers
{
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [Route("home")]
        public string Index()
        {
            return "Hello world";
        }
    }
}
