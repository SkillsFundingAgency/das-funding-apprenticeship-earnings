using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.Scan(scan =>
                {
                    scan.FromAssembliesOf(typeof(ServiceCollectionExtensions))
                        .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime();
                })
                .AddScoped<IDomainEventDispatcher, DomainEventDispatcher>()
                .AddScoped<IAcademicYearService, AcademicYearService>()
                .AddScoped<IDateService, DateService>();

            return serviceCollection;
        }
    }
}
