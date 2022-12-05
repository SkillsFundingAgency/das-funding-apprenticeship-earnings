using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandServices(this IServiceCollection serviceCollection)
        {

            serviceCollection.AddPersistenceServices();

            return serviceCollection;
        }

        private static IServiceCollection AddPersistenceServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IEarningsQueryRepository, EarningsQueryRepository>();
            return serviceCollection;
        }
    }
}
