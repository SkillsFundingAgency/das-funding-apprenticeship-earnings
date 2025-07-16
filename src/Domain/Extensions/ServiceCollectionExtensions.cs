using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAcademicYearService, AcademicYearService>();
        serviceCollection.AddScoped<IDateService, DateService>();
        return serviceCollection;
    }
}
