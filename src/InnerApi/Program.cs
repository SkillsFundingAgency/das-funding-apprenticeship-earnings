using Microsoft.Extensions.Logging.ApplicationInsights;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Configuration.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Health;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddAzureTableStorage(options =>
{
    options.ConfigurationKeys = new[] { "SFA.DAS.Funding.ApprenticeshipEarnings" };
    options.StorageConnectionString = builder.Configuration["ConfigurationStorageConnectionString"];
    options.EnvironmentName = builder.Configuration["EnvironmentName"];
    options.PreFixConfigurationKeys = false;
});

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddApplicationInsights();
    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var applicationSettings = new ApplicationSettings();
var sqlConnectionNeedsAccessToken = NotLocal(builder.Configuration);
builder.Configuration.Bind(nameof(ApplicationSettings), applicationSettings);
builder.Services.AddEntityFrameworkForApprenticeships(applicationSettings, sqlConnectionNeedsAccessToken);
builder.Services.AddSingleton(x => applicationSettings);
builder.Services.ConfigureNServiceBusForSend(applicationSettings.NServiceBusConnectionString.GetFullyQualifiedNamespace());
builder.Services.AddQueryServices().AddCommandDependencies().AddEventServices().AddCommandServices();
builder.Services.AddApplicationHealthChecks(applicationSettings, sqlConnectionNeedsAccessToken);


//Add MI authentication
if (NotLocal(builder.Configuration))
{
    var azureAdConfiguration = builder.Configuration
        .GetSection("AzureAd")
        .Get<AzureActiveDirectoryConfiguration>();

    var policies = new Dictionary<string, string>
    {
        {PolicyNames.Default, "Default"}
    };

    builder.Services.AddAuthentication(azureAdConfiguration, policies);
}

builder.Services.AddMvc(o =>
{
    if (NotLocal(builder.Configuration))
    {
        o.Conventions.Add(new AuthorizeControllerModelConvention(new List<string>()));
    }
});

var app = builder.Build();

app.MapHealthChecks("/ping");   // Both /ping 
app.MapHealthChecks("/");       // and / are used for health checks

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static bool NotLocal(IConfiguration configuration)
{
    return !configuration!["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase);
}