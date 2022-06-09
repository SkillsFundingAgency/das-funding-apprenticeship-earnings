using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableFunctions;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Acceptance;

public class TestFunction : IDisposable
{
    public const int BusinessCentralPaymentRequestsLimit = 1;
    private readonly TestContext _testContext;
    private readonly Dictionary<string, string> _appConfig;
    private readonly IHost _host;
    private readonly OrchestrationData _orchestrationData;
    private bool isDisposed;

    private IJobHost Jobs => _host.Services.GetService<IJobHost>();
    public string HubName { get; }
    public HttpResponseMessage LastResponse => ResponseObject as HttpResponseMessage;
    public ObjectResult HttpObjectResult => ResponseObject as ObjectResult;
    public object ResponseObject { get; private set; }

    public TestFunction(TestContext testContext, string hubName)
    {
        HubName = hubName;
        _orchestrationData = new OrchestrationData();

        _testContext = testContext;

        _appConfig = new Dictionary<string, string>{ //todo this needs to match our durable function config
            { "EnvironmentName", "LOCAL_ACCEPTANCE_TESTS" },
            { "AzureWebJobsStorage", "UseDevelopmentStorage=true" },
            { "NServiceBusConnectionString", "UseDevelopmentStorage=true" },
            { "ConfigNames", "SFA.DAS.EmployerIncentives" },
            { "ApplicationSettings:LogLevel", "Info" },
            { "ApplicationSettings:DbConnectionString", _testContext.SqlDatabase.DatabaseInfo.ConnectionString },
        };

        _testContext = testContext;

        //Microsoft.Extensions.Hosting.WebJobsHostBuilderExtensions

        _host = new HostBuilder()
            .ConfigureAppConfiguration(a =>
            {
                a.Sources.Clear();
                a.AddInMemoryCollection(_appConfig);
            })
            .ConfigureWebJobs(builder => builder
                //.AddHttp(options => options.SetResponse = (request, o) =>
                //{
                //    ResponseObject = o;
                //}) todo what is this used for and do we need it, seemingly no longer supported in latest version of Microsoft.Extensions.Hosting
                .AddDurableTask(options =>
                {
                    options.HubName = HubName;
                    options.UseAppLease = false;
                    options.UseGracefulShutdown = false;
                    options.ExtendedSessionsEnabled = false;
                    options.StorageProvider["maxQueuePollingInterval"] = new TimeSpan(0, 0, 0, 0, 500);
                    options.StorageProvider["partitionCount"] = 1;
                    //options.NotificationUrl = new Uri("localhost:7071"); todo again no longer supported
#pragma warning disable S125 // Sections of code should not be commented out
                    //options.StorageProvider["controlQueueBatchSize"] = 5;
                    //options.HttpSettings.DefaultAsyncRequestSleepTimeMilliseconds = 500;
                    //options.MaxConcurrentActivityFunctions = 10;
                    //options.MaxConcurrentOrchestratorFunctions = 5;
#pragma warning restore S125
                })
                .AddAzureStorageCoreServices()
                .ConfigureServices(s =>
                {
                    new Startup().Configure(builder);


                    //todo what if any extra configure calls do we need here
                    //s.Configure<MatchedLearnerApi>(l =>
                    //{
                    //    l.ApiBaseUrl = _testContext.LearnerMatchApi.BaseAddress;
                    //    l.Identifier = "";
                    //    l.Version = "1.0";
                    //});

                    //s.Configure<BusinessCentralApiClient>(c =>
                    //{
                    //    c.ApiBaseUrl = _testContext.PaymentsApi.BaseAddress;
                    //    c.PaymentRequestsLimit = BusinessCentralPaymentRequestsLimit;
                    //});

                    //s.Configure<ApplicationSettings>(a =>
                    //{
                    //    a.DbConnectionString = _testContext.SqlDatabase.DatabaseInfo.ConnectionString;
                    //    a.DistributedLockStorage = _testContext.ApplicationSettings.DistributedLockStorage;
                    //    a.LockedRetryPolicyInMilliSeconds = _testContext.ApplicationSettings.LockedRetryPolicyInMilliSeconds;
                    //    a.AllowedHashstringCharacters = _testContext.ApplicationSettings.AllowedHashstringCharacters;
                    //    a.Hashstring = _testContext.ApplicationSettings.Hashstring;
                    //    a.NServiceBusConnectionString = _testContext.ApplicationSettings.NServiceBusConnectionString;
                    //    a.NServiceBusLicense = _testContext.ApplicationSettings.NServiceBusLicense;
                    //    a.UseLearningEndpointStorageDirectory = _testContext.ApplicationSettings.UseLearningEndpointStorageDirectory;
                    //    a.MinimumAgreementVersion = _testContext.ApplicationSettings.MinimumAgreementVersion;
                    //    a.ApiBaseUrl = _testContext.ApplicationSettings.ApiBaseUrl;
                    //    a.Identifier = _testContext.ApplicationSettings.Identifier;
                    //    a.EmployerIncentivesWebBaseUrl = _testContext.ApplicationSettings.EmployerIncentivesWebBaseUrl;
                    //    a.LogLevel = _testContext.ApplicationSettings.LogLevel;
                    //    a.EmploymentCheckEnabled = _testContext.ApplicationSettings.EmploymentCheckEnabled;
                    //    a.LearnerServiceCacheIntervalInMinutes = _testContext.ApplicationSettings.LearnerServiceCacheIntervalInMinutes;
                    //});

                    //s.AddSingleton<IDistributedLockProvider, TestDistributedLockProvider>();
                    s.AddSingleton(typeof(IOrchestrationData), _orchestrationData);
                    //s.Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithTimings<>));
                })
            )
            .ConfigureServices(s =>
            {
                s.AddHostedService<PurgeBackgroundJob>();
            })
            .Build();
    }

    public async Task StartHost()
    {
        var timeout = new TimeSpan(0, 0, 10);
        var delayTask = Task.Delay(timeout);
        await Task.WhenAny(Task.WhenAll(_host.StartAsync(), Jobs.Terminate()), delayTask);

        if (delayTask.IsCompleted)
        {
            throw new Exception($"Failed to start test function host within {timeout.Seconds} seconds.  Check the AzureStorageEmulator is running. ");
        }
    }

    public Task Start(OrchestrationStarterInfo starter, bool throwIfFailed = true)
    {
        return Jobs.Start(starter, throwIfFailed);
    }

    public async Task<ObjectResult> CallEndpoint(EndpointInfo endpoint)
    {
        await Jobs.Start(endpoint);
        return ResponseObject as ObjectResult;
    }


    //todo I think this object might be different for us to just returning the raw string for now
    public async Task<string> GetOrchestratorStartResponse()
    {
        var responseString = await LastResponse.Content.ReadAsStringAsync();
        //var responseValue = JsonConvert.DeserializeObject<OrchestratorStartResponse>(responseString);
        return responseString;
    }
    //public async Task<OrchestratorStartResponse> GetOrchestratorStartResponse()
    //{
    //    var responseString = await LastResponse.Content.ReadAsStringAsync();
    //    var responseValue = JsonConvert.DeserializeObject<OrchestratorStartResponse>(responseString);
    //    return responseValue;
    //}

    public async Task<DurableOrchestrationStatus> GetStatus(string instanceId)
    {
        await Jobs.RefreshStatus(instanceId);
        return _orchestrationData.Status;
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
        if (isDisposed) return;

        if (disposing)
        {
            _host.Dispose();
        }

        isDisposed = true;
    }
}