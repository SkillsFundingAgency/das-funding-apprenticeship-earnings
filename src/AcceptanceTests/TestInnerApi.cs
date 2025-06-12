using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NServiceBus;
using NServiceBus.Testing;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Configuration.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries;
using SFA.DAS.Testing.AzureStorageEmulator;
using System.Text;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

public class TestInnerApi : IDisposable
{
    private readonly TestContext _testContext;
    private readonly TestServer _testServer;
    private readonly HttpClient _httpClient;
    private readonly IEnumerable<MessageHandler> _queueTriggeredFunctions;
    private bool _isDisposed;


    public TestInnerApi(TestContext testContext)
    {
        _testContext = testContext;

        var builder = new WebHostBuilder()
            .UseEnvironment("SystemAcceptanceTests")
            .ConfigureServices(services =>
            {
                services.AddControllers()
                    .AddApplicationPart(typeof(SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers.ApprenticeshipController).Assembly);
                
                services.AddQueryServices().AddCommandDependencies().AddEventServices().AddCommandServices();
                services.AddSingleton<IMessageSession>(_testContext.MessageSession);

                var applicationSettings = new ApplicationSettings
                {
                    SqlConnectionString = testContext.SqlDatabase?.DatabaseInfo.ConnectionString
                };

                services.AddTransient<ApplicationSettings>(provider => applicationSettings);
                services.AddDbContext<ApprenticeshipEarningsDataContext>(ServiceLifetime.Transient);
                services.AddScoped(provider => new Lazy<ApprenticeshipEarningsDataContext>(provider.GetService<ApprenticeshipEarningsDataContext>()!));

            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });

        _testServer = new TestServer(builder);
        _httpClient = _testServer.CreateClient();

    }

   

    public async Task Patch<T>(string route, T body)
    {
        var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync(route, content);
    }

    public async Task Post<T>(string route, T body)
    {
        var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(route, content);
    }

    public async Task PublishEvent<T>(T eventObject)
    {
        var response = await _httpClient.GetAsync("/home");
        var responseString = await response.Content.ReadAsStringAsync();
    }


    public async Task DisposeAsync()
    {
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
            _testServer.Dispose();
            _httpClient.Dispose();
        }

        _isDisposed = true;
    }

}