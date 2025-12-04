using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

public class TestContext : IDisposable
{
    public TestFunction? TestFunction { get; set; }
    public TestInnerApi? TestInnerApi { get; set; }
    public SqlDatabase? SqlDatabase { get; set; }
    public TestMessageSession MessageSession { get; set; }
    public ITestFundingBandMaximumService FundingBandMaximumService { get; set; }

    public void Dispose()
    {
        TestFunction?.Dispose();
        TestInnerApi?.Dispose();
        SqlDatabase?.Dispose();
    }

    public TestContext()
    {
        MessageSession = new TestMessageSession();
        FundingBandMaximumService = new TestFundingBandMaximumService();
    }

}
