using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Acceptance
{
    public class TestContext : IDisposable
    {
        public TestFunction TestFunction { get; set; }
        public SqlDatabase SqlDatabase { get; set; }
        public void Dispose()
        {
            //throw new NotImplementedException(); //todo stop function?
        }
    }
}
