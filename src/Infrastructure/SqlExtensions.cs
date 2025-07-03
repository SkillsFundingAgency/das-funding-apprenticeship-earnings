namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;

public static class SqlExtensions
{
    private static string AzureActiveDirectory = "Authentication=Active Directory Default";

    public static string EnsureAzureAdAuthentication(this string originalConnectionString, bool requiresAzureAuthentication)
    {
        var connectionString = originalConnectionString;
        if (requiresAzureAuthentication && !connectionString.Contains(AzureActiveDirectory))
        {
            connectionString += $";{AzureActiveDirectory}";
        }

        return connectionString;
    }
}
