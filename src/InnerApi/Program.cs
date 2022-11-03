using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddAzureTableStorage(options =>
{
    options.ConfigurationKeys = new[] { "SFA.DAS.Funding.ApprenticeshipEarnings" };
    options.StorageConnectionString = builder.Configuration["ConfigurationStorageConnectionString"];
    options.EnvironmentName = builder.Configuration["EnvironmentName"];
    options.PreFixConfigurationKeys = false;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApprenticeshipEarningsDataContext>();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/ping");

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
