using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;

public class GetFm36DataQueryHandler : IQueryHandler<GetFm36DataRequest, GetFm36DataResponse>
{
    private readonly IApiClient<DurableEntityApiConfig> _apiClient;

    public GetFm36DataQueryHandler(IApiClient<DurableEntityApiConfig> apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<GetFm36DataResponse> Handle(GetFm36DataRequest query, CancellationToken cancellationToken = default)
    {
        var apiResponse = await _apiClient.Get<GetFm36DataResponse>(query);

        if (!apiResponse.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to get FM36 data for UKPRN {query.Ukprn}. Status code: {apiResponse.StatusCode}. Error content: {apiResponse.ErrorContent}");
        }

        var data = apiResponse.Body;

        if(data == null)
        {
            throw new HttpRequestException($"Failed to get FM36 data for UKPRN {query.Ukprn}. Response body is null.");
        }

        return data;
    }

}