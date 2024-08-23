namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api.Configuration;

public interface IApiConfig
{
    public string BaseUrl { get; set; }
}

public interface IAzureFunctionApiConfig : IApiConfig
{
    public string AppKey { get; set; }
}
