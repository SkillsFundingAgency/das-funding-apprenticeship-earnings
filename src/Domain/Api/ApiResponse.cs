using System.Net;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api;

public class ApiResponse<TResponse>
{
    public TResponse? Body { get; }
    public HttpStatusCode StatusCode { get; }
    public bool IsSuccessStatusCode => (int)StatusCode >= 200 && (int)StatusCode <= 299;
    public string ErrorContent { get; }

    public ApiResponse(TResponse? body, HttpStatusCode statusCode, string errorContent)
    {
        Body = body;
        StatusCode = statusCode;
        ErrorContent = errorContent;
    }
}
