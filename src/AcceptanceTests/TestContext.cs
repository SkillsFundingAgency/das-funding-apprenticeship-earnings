namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests
{
    public class TestContext : IDisposable
    {
        public TestFunction? TestFunction { get; set; }

        public void Dispose()
        {
            TestFunction?.Dispose();
        }
    }
}
