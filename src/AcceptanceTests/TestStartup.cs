using Grpc.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

internal class TestStartup
{
    private readonly Startup _startUp;
    private readonly IEnumerable<MessageHandler> _queueTriggeredFunctions;
    private readonly IMessageSession _messageSession;
    private readonly IFundingBandMaximumService _fundingBandMaximumService;

    public TestStartup(
        TestContext testContext, 
        IEnumerable<MessageHandler> queueTriggeredFunctions,
        IMessageSession messageSession)
    {
        _startUp = new Startup();
        _startUp.Configuration = GenerateConfiguration(testContext);
        _queueTriggeredFunctions = queueTriggeredFunctions;
        _messageSession = messageSession;
        _fundingBandMaximumService = testContext.FundingBandMaximumService;
    }

    public void Configure()
    {
        // Intentionally left blank
    }

    public void ConfigureServices(IServiceCollection collection)
    {
        _startUp.SetupServices(collection);

        collection.AddSingleton<IMessageSession>(_messageSession);

        foreach (var queueTriggeredFunction in _queueTriggeredFunctions)
        {
            collection.AddScoped(queueTriggeredFunction.HandlerType);
        }

        collection.AddSingleton<ISystemClockService, TestSystemClock>();// override DI in Startup

        collection.AddSingleton<IFundingBandMaximumService>(_fundingBandMaximumService);
    }

    private static IConfigurationRoot GenerateConfiguration(TestContext testContext)
    {
        var configSource = new MemoryConfigurationSource
        {
            InitialData = new[]
            {
                new KeyValuePair<string, string>("EnvironmentName", "LOCAL_ACCEPTANCE_TESTS"),
                new KeyValuePair<string, string>("AzureWebJobsStorage", "UseDevelopmentStorage=true"),
                new KeyValuePair<string, string>("AzureWebJobsServiceBus", "UseDevelopmentStorage=true"),
                new KeyValuePair<string, string>("ApplicationSettings:NServiceBusConnectionString", "UseLearningEndpoint=true"),
                new KeyValuePair<string, string>("ApplicationSettings:DbConnectionString", testContext.SqlDatabase?.DatabaseInfo.ConnectionString!)
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);
        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }
}
