using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Funding.ApprenticeshipEarnings.Funcs31;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SFA.DAS.Funding.ApprenticeshipEarnings.Funcs31
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddOptions()
                ;

            var serviceProvider = builder.Services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            configBuilder.AddJsonFile("local.settings.json", optional: true);

            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));
          
            
            var applicationSettings = config.GetSection("Values").Get<ServiceBusConfiguration>();

            builder.Services
                .AddNServiceBus(applicationSettings)
                .AddNServiceBusMessageHandlers(applicationSettings)
                ;
        }
    }
}