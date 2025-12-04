using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;


public interface ITestFundingBandMaximumService : IFundingBandMaximumService
{
    void SetFundingBandMaximum(int fundingBandMaximum);

    int GetFundingBandMaximum();
}

internal class TestFundingBandMaximumService : ITestFundingBandMaximumService
{
    private int _fundingBandMaximum = SharedDefaults.FundingBandMaximum;

    public void SetFundingBandMaximum(int fundingBandMaximum)
    {
        _fundingBandMaximum = fundingBandMaximum;
    }
    public int GetFundingBandMaximum()
    {
        return _fundingBandMaximum;
    }

    public Task<int?> GetFundingBandMaximum(string courseCode, DateTime? startDate)
    {
        return Task.FromResult<int?>(_fundingBandMaximum);
    }
}
