using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.InternalEvents;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Acceptance.StepDefinitions;

[Binding]
public class HostingStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private readonly FeatureContext _featureContext;
    private static IConfiguration _config;

    public HostingStepDefinitions(ScenarioContext scenarioContext, TestContext testContext, FeatureContext featureContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
        _featureContext = featureContext;
    }

    [BeforeScenario]
    public async Task CreateConfig()
    {
        if (_config == null)
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
                .Build();
        }

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

    [Given(@"An apprenticeship learner event")]
    public async Task PublishApprenticeshipLearnerEvent()
    {
        var endpointConfiguration = new EndpointConfiguration(QueueNames.ApprenticeshipLearners);
        endpointConfiguration.AssemblyScanner().ThrowExceptions = false;
        endpointConfiguration.SendOnly();
        endpointConfiguration.UseNewtonsoftJsonSerializer();

        var transport = endpointConfiguration.UseTransport<LearningTransport>();
        transport.StorageDirectory("C:\\temp\\LearningTransport\\FPAY-14");

        IEndpointInstance endpointInstance;

        try
        {
            endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            await endpointInstance.Publish(new InternalApprenticeshipLearnerEvent()
            { AgreedPrice = 11000 });

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
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

    //private bool DoesTagExist(string tag)
    //{
    //    var tags = _scenarioContext.ScenarioInfo.Tags;
    //    return tags.Any(x => x.Equals(tag));
    //}
}