﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary;

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
                    scan.FromExecutingAssembly()
                        .AddClasses(classes => classes.AssignableTo(typeof(IQuery)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime();

                    scan.FromAssembliesOf(typeof(GetProviderEarningSummaryQueryHandler))
                        .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime();
                })
                .AddTransient<IEarningsQueryRepository, EarningsQueryRepository>()
                .AddScoped<IQueryDispatcher, QueryDispatcher>()
                .AddTransient<ISystemClockService, SystemClockService>();

            return serviceCollection;
        }
    }
}
