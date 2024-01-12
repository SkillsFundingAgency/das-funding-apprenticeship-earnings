using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkForApprenticeships(this IServiceCollection services, ApplicationSettings settings, bool connectionNeedsAccessToken)
        {
            services.AddSingleton<ISqlAzureIdentityTokenProvider, SqlAzureIdentityTokenProvider>();

            services.AddSingleton(provider => new SqlAzureIdentityAuthenticationDbConnectionInterceptor(provider.GetService<ILogger<SqlAzureIdentityAuthenticationDbConnectionInterceptor>>(), provider.GetService<ISqlAzureIdentityTokenProvider>(), connectionNeedsAccessToken));

            services.AddScoped(p =>
            {
                var options = new DbContextOptionsBuilder<ApprenticeshipEarningsDataContext>()
                    .UseSqlServer(new SqlConnection(settings.DbConnectionString), optionsBuilder => optionsBuilder.CommandTimeout(7200)) //7200=2hours
                    .AddInterceptors(p.GetRequiredService<SqlAzureIdentityAuthenticationDbConnectionInterceptor>())
                    .Options;
                return new ApprenticeshipEarningsDataContext(options);
            });

            return services.AddScoped(provider =>
            {
                var dataContext = provider.GetService<ApprenticeshipEarningsDataContext>() ?? throw new ArgumentNullException("ApprenticeshipEarningsDataContext");
                return new Lazy<ApprenticeshipEarningsDataContext>(dataContext);
            });
        }
    }
}
