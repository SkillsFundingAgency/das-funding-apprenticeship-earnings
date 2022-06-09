using System.Diagnostics;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Acceptance.StepDefinitions;

[Binding]
public class HostingStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private readonly FeatureContext _featureContext;

    public HostingStepDefinitions(ScenarioContext scenarioContext, TestContext testContext, FeatureContext featureContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
        _featureContext = featureContext;
    }

    [Given(@"Fire test harness")]
    public async Task GivenFireTestHarness()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _testContext.TestFunction = new TestFunction(_testContext, $"TEST{_featureContext.FeatureInfo.Title}");
        await _testContext.TestFunction.StartHost();
        stopwatch.Stop();
        Console.WriteLine($"Time it took to spin up Azure Functions Host: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.TestFunction.HubName}");
    }

    [AfterScenario()]
    public async Task CleanupAfterTestHarness()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await _testContext.TestFunction.DisposeAsync();
        stopwatch.Stop();
        Console.WriteLine($"Time it took to Cleanup  FunctionsHost: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.TestFunction.HubName}");
    }

    //[BeforeTestRun]
    //public void SetupInMemoryDatabase()
    //{
    //    if (DoesTagExist(AcceptanceTestTags.RequiresDatabase))
    //    {
                
    //    }
    //}

    //[BeforeTestRun]
    //public void SetupApi()
    //{
    //    if (DoesTagExist(AcceptanceTestTags.RequiresApi))
    //    {
                
    //    }
    //}

    //[BeforeTestRun]
    //public async Task SetupFunctions()
    //{
    //    if (DoesTagExist(AcceptanceTestTags.RequiresFunctions))
    //    {
    //        await InitialiseHost();
    //    }
    //}

    //private async Task InitialiseHost()
    //{
    //    var stopwatch = new Stopwatch();
    //    stopwatch.Start();
    //    _testContext.TestFunction = new TestFunction(_testContext, $"TEST{_featureContext.FeatureInfo.Title}");
    //    await _testContext.TestFunction.StartHost();
    //    stopwatch.Stop();
    //    Console.WriteLine($"Time it took to spin up Azure Functions Host: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.TestFunction.HubName}");
    //}

    private bool DoesTagExist(string tag)
    {
        var tags = _scenarioContext.ScenarioInfo.Tags;
        return tags.Any(x => x.Equals(tag));
    }
}