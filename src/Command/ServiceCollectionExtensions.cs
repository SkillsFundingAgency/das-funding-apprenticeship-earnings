using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandServices(this IServiceCollection serviceCollection)
    {

        serviceCollection
            .AddCommandHandlers()
            .AddScoped<ICommandDispatcher, CommandDispatcher>()
            .AddPersistenceServices();

        return serviceCollection;
    }

    public static IServiceCollection AddCommandDependencies(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<IApprenticeshipFactory, ApprenticeshipFactory>()
            .AddSingleton<IEarningsGeneratedEventBuilder, EarningsGeneratedEventBuilder>();
        return serviceCollection;
    }

    private static IServiceCollection AddPersistenceServices(this IServiceCollection serviceCollection)
    {

        serviceCollection.AddScoped<IApprenticeshipRepository, ApprenticeshipRepository>();
        serviceCollection.AddScoped<IEarningsQueryRepository, EarningsQueryRepository>();
        serviceCollection.AddScoped<IEarningsProfileHistoryRepository, EarningsProfileHistoryRepository>();
        return serviceCollection;
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection)
    {
        serviceCollection.Scan(scan =>
        {
            scan.FromAssembliesOf(typeof(ServiceCollectionExtensions))
                .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime();

            scan.FromAssembliesOf(typeof(ServiceCollectionExtensions))
                .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime();
        });

        return serviceCollection;
    }

}
