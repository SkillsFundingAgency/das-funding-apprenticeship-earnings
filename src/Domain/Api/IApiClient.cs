namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api;

public interface IApiClient<T> where T : IApiConfig
{
    Task<ApiResponse<TResponse>> Get<TResponse>(IGetApiRequest request);
}
