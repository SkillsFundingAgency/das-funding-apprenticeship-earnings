using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;

[ExcludeFromCodeCoverage]
public class SqlAzureIdentityTokenProvider : ISqlAzureIdentityTokenProvider
{
    public async Task<string> GetAccessTokenAsync()
    {
        var tokenCredential = new DefaultAzureCredential();
        var token = await tokenCredential.GetTokenAsync(
            new TokenRequestContext(scopes: new[] { "https://database.windows.net" + "/.default" }));

        return token.Token;
    }

    public string GetAccessToken()
    {
        var tokenCredential = new DefaultAzureCredential();
        var token = tokenCredential.GetToken(
            new TokenRequestContext(scopes: new[] { "https://database.windows.net" + "/.default" }));
        
        return token.Token;
    }
}