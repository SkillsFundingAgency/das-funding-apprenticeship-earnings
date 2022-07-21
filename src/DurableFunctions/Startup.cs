using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(SFA.DAS.Funding.ApprenticeshipEarnings.DurableFunctions.Startup))]
namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //throw new NotImplementedException();
            //todo IoC etc.
        }
    }
}
