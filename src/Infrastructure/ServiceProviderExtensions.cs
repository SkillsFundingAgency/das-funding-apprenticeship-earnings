using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Only returns token provider if required, else returns null
    /// </summary>
    public static ISqlAzureIdentityTokenProvider? GetSqlAzureIdentityTokenProvider(this IServiceProvider services, bool sqlConnectionNeedsAccessToken)
    {
        if (!sqlConnectionNeedsAccessToken)
            return null;

        return services.GetService<ISqlAzureIdentityTokenProvider>();
    }
}
