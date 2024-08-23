using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api.Requests;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api.Clients;

public interface IApiClient<T> where T : IApiConfig
{
    Task<ApiResponse<TResponse>> Get<TResponse>(IGetApiRequest request);
}
