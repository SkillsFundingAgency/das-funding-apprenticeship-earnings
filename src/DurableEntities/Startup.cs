﻿using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;

[assembly: FunctionsStartup(typeof(SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Startup))]
namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;

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
            .AddJsonFile("local.settings.json");

        builder.Services.AddSingleton<IAdjustedPriceProcessor, AdjustedPriceProcessor>();
        builder.Services.AddSingleton<IInstallmentsGenerator, InstallmentsGenerator>();
        builder.Services.AddSingleton<IEarningsProfileGenerator, EarningsProfileGenerator>();
        builder.Services.AddSingleton<IEarningsGeneratedEventBuilder, EarningsGeneratedEventBuilder>();

        Configuration = configBuilder.Build();

        builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), Configuration));

        builder.Services.AddNServiceBus(Configuration);
    }
}