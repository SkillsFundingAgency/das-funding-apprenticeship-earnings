using System.Text.Json.Serialization;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api.Requests;

public interface IGetApiRequest
{
    /// <summary>
    /// This is the relative URL used in the GET request. Note that this should not have a leading slash.
    /// </summary>
    [JsonIgnore]
    string GetUrl { get; }
}
