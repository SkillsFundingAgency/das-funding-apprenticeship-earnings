using Microsoft.Extensions.Options;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api.Configuration;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api.Clients;

public class AzureFunctionApiClient<T> : ApiClient<T> where T : class, IAzureFunctionApiConfig, new()
{
    private readonly string _appKey;

    public AzureFunctionApiClient(HttpClient httpClient, IOptions<T> config) : base(httpClient, config)
    {
        _appKey = config.Value.AppKey;
    }

    protected override void AddAuthHeaders(HttpRequestMessage request)
    {
        request.Headers.Add("x-functions-key", _appKey);
    }
}
