using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApprovePriceChangeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;

[assembly: FunctionsStartup(typeof(SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Startup))]
namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;

[ExcludeFromCodeCoverage]
public class Startup : FunctionsStartup
{
    public IConfiguration Configuration { get; set; }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        var serviceProvider = builder.Services.BuildServiceProvider();
            
        var configuration = serviceProvider.GetService<IConfiguration>();

        var configBuilder = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables()
            .AddJsonFile("local.settings.json", true);

        if (!configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase))
        {
            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });
        }

        Configuration = configBuilder.Build();

        var applicationSettings = new ApplicationSettings();
        Configuration.Bind(nameof(ApplicationSettings), applicationSettings);
        EnsureConfig(applicationSettings);
        Environment.SetEnvironmentVariable("NServiceBusConnectionString", applicationSettings.NServiceBusConnectionString);
       
        builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), Configuration));
        builder.Services.AddSingleton<ApplicationSettings>(x => applicationSettings);

        builder.Services.AddNServiceBus(applicationSettings);
        builder.Services.AddEntityFrameworkForApprenticeships(applicationSettings, NotAcceptanceTests(configuration));
        builder.Services.AddCommandServices().AddEventServices();

        builder.Services.AddSingleton<ISystemClockService, SystemClockService>();
        builder.Services.AddSingleton<IInstalmentsGenerator, InstalmentsGenerator>();
        builder.Services.AddSingleton<IEarningsGeneratedEventBuilder, EarningsGeneratedEventBuilder>();
        builder.Services.AddSingleton<IApprenticeshipEarningsRecalculatedEventBuilder, ApprenticeshipEarningsRecalculatedEventBuilder>();
        builder.Services.AddScoped<ICreateApprenticeshipCommandHandler, CreateApprenticeshipCommandHandler>();
        builder.Services.AddScoped<IApprovePriceChangeCommandHandler, ApprovePriceChangeCommandHandler>();
        builder.Services.AddScoped<IApproveStartDateChangeCommandHandler, ApproveStartDateChangeCommandHandler>();
        builder.Services.AddScoped<IApprenticeshipFactory, ApprenticeshipFactory>();
    }

    private static void EnsureConfig(ApplicationSettings applicationSettings)
    {
        if (string.IsNullOrWhiteSpace(applicationSettings.NServiceBusConnectionString))
            throw new Exception("NServiceBusConnectionString in ApplicationSettings should not be null.");
    }

    private static bool NotAcceptanceTests(IConfiguration configuration)
    {
        return !configuration!["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase);
    }
}