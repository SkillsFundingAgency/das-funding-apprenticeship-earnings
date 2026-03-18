using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueryServices(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .Scan(scan =>
                {
                    scan.FromAssembliesOf(typeof(GetFm36DataQueryHandler))
                        .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime();
                })
                .AddScoped<IQueryDispatcher, QueryDispatcher>();

            return serviceCollection;
        }
    }
}
