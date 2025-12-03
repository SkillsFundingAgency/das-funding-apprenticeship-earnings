using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

internal class TestFundingBandMaximumService : IFundingBandMaximumService
{
    public Task<int?> GetFundingBandMaximum(string courseCode, DateTime? startDate)
    {
        return Task.FromResult<int?>(500);
    }
}
