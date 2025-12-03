using Microsoft.Extensions.Configuration;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;

public static class ConfigurationExtensions
{
    public static bool NotLocal(this IConfiguration configuration)
    {
        return IsNotEnvironmentName(configuration, "LOCAL");
    }

    public static bool NotAcceptanceTests(this IConfiguration configuration)
    {
        return IsNotEnvironmentName(configuration, "LOCAL_ACCEPTANCE_TESTS");
    }

    private static bool IsNotEnvironmentName(IConfiguration configuration, string expectedEnvironmentName)
    {
        var actualEnvironmentName = configuration["EnvironmentName"];
        if (actualEnvironmentName is null)
            return true;

        return !actualEnvironmentName.Equals(expectedEnvironmentName, StringComparison.CurrentCultureIgnoreCase);
    }
}
