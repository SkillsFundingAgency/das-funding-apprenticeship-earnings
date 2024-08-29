using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Testing.AzureStorageEmulator;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

public class Settings
{
    public string AzureWebJobsStorage { get; set; }
    public string NServiceBusConnectionString { get; set; }
    public string TopicPath { get; set; }
    public string QueueName { get; set; }
}

public class TestFunction : IDisposable
{
    private readonly TestContext _testContext;
    private readonly IHost _host;
    private bool _isDisposed;

    private IJobHost Jobs => _host.Services.GetService<IJobHost>();
    public string HubName { get; }
    private readonly OrchestrationData _orchestrationData;

    public TestFunction(TestContext testContext, string hubName)
    {
        AzureStorageEmulatorManager.StartStorageEmulator();
        
        HubName = hubName;
        _orchestrationData = new OrchestrationData();

        _testContext = testContext;

        
        var config = new ConfigurationBuilder()
            //.SetBasePath()
            .AddJsonFile("local.settings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var settings = new Settings();

        config.Bind(settings);

        var appConfig = new Dictionary<string, string>{
            { "EnvironmentName", "LOCAL_ACCEPTANCE_TESTS" },
            { "AzureWebJobsStorage", settings.AzureWebJobsStorage },
            { "NServiceBusConnectionString", settings.NServiceBusConnectionString ?? "UseLearningEndpoint=true" },
            { "TopicPath", settings.TopicPath },
            { "QueueName", settings.QueueName },
            { "ApplicationSettings:LogLevel", "DEBUG" },
            { "ApplicationSettings:DbConnectionString", testContext.SqlDatabase?.DatabaseInfo.ConnectionString! }
        };

        _testContext = testContext;

        Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage=true", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("NServiceBusConnectionString", "UseLearningEndpoint=true", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("ApplicationSettings:NServiceBusConnectionString", "UseLearningEndpoint=true", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("LearningTransportStorageDirectory", Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), @"src\.learningtransport"), EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("EnvironmentName", "LOCAL_ACCEPTANCE_TESTS", EnvironmentVariableTarget.Process);

        _host = new HostBuilder()
            .ConfigureAppConfiguration(a =>
            {
                a.Sources.Clear();
                a.AddInMemoryCollection(appConfig);
            })
            .ConfigureWebJobs(builder => builder
                .AddDurableTask(options =>
                {
                    options.HubName = HubName;
                    options.UseAppLease = false;
                    options.UseGracefulShutdown = false;
                    options.ExtendedSessionsEnabled = false;
                    options.StorageProvider["maxQueuePollingInterval"] = new TimeSpan(0, 0, 0, 0, 500);
                    options.StorageProvider["partitionCount"] = 1;
                })
                .AddAzureStorageCoreServices()
                .ConfigureServices(s =>
                {
                    builder.Services.AddLogging(options =>
                    {
                        options.SetMinimumLevel(LogLevel.Trace);
                        options.AddConsole();
                    });
                    s.Configure<ApplicationSettings>(a =>
                    {
                        a.AzureWebJobsStorage = appConfig["AzureWebJobsStorage"];
                        a.QueueName = appConfig["QueueName"];
                        a.TopicPath = appConfig["TopicPath"];
                        a.ServiceBusConnectionString = appConfig["NServiceBusConnectionString"];
                        a.DbConnectionString = appConfig["DbConnectionString"];
                    });

                    new Startup().Configure(builder);


                    s.AddSingleton(typeof(IOrchestrationData), _orchestrationData);
                    s.AddSingleton<ISystemClockService, TestSystemClock>();// override DI in Startup
                })
            )
            .Build();
    }

    public async Task StartHost()
    {
        var timeout = new TimeSpan(0, 2, 10);
        var delayTask = Task.Delay(timeout);

        await Task.WhenAny(Task.WhenAll(_host.StartAsync()), delayTask);

        if (delayTask.IsCompleted)
        {
            throw new Exception($"Failed to start test function host within {timeout.Seconds} seconds.  Check the AzureStorageEmulator is running. ");
        }
    }
    
    public async Task DisposeAsync()
    {
        await Jobs.StopAsync();
        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            _host.Dispose();
        }

        _isDisposed = true;
    }
}