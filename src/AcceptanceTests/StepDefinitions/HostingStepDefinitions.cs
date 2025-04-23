using Azure;
using Castle.Components.DictionaryAdapter.Xml;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;
using System;
using System.Diagnostics;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class HostingStepDefinitions
{
    private readonly TestContext _testContext;
    private readonly FeatureContext _featureContext;
    private static Startup _startUp;

    public HostingStepDefinitions(TestContext testContext, FeatureContext featureContext)
    {
        _testContext = testContext;
        _featureContext = featureContext;
    }

    [BeforeScenario(Order = 3)]
    public async Task StartFunctionApp()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _testContext.TestFunction = new TestFunction(_testContext, $"TEST{_featureContext.FeatureInfo.Title.Replace(" ", "")}");
        _testContext.TestInnerApi = new TestInnerApi(_testContext);
        stopwatch.Stop();
        Console.WriteLine($"Time it took to spin up Azure Functions Host: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.TestFunction.HubName}");
    }

    [AfterScenario(Order = 101)]
    public async Task CleanupAfterTestHarness()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await _testContext.TestFunction.DisposeAsync();
        await _testContext.TestInnerApi.DisposeAsync();
        stopwatch.Stop();
        Console.WriteLine($"Time it took to Cleanup  FunctionsHost: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.TestFunction.HubName}");
    }
}