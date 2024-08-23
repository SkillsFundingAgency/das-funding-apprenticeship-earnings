namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api.Configuration;

public class DurableEntityApiConfig : IAzureFunctionApiConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string AppKey { get; set; } = string.Empty;
}
