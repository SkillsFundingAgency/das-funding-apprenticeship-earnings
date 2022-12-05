namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;

public interface ISqlAzureIdentityTokenProvider
{
    Task<string> GetAccessTokenAsync();
    string GetAccessToken();
}