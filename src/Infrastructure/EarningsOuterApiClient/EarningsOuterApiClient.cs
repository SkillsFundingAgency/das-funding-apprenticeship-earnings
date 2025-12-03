using Newtonsoft.Json;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.EarningsOuterApiClient.Models;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.EarningsOuterApiClient;

public interface IEarningsOuterApiClient
{
    Task<GetStandardResponse> GetStandard(string courseCode);
}

[ExcludeFromCodeCoverage]
public class EarningsOuterApiClient : IEarningsOuterApiClient
{
    private readonly HttpClient _httpClient;

    private const string GetStandardUrl = "TrainingCourses/standards";

    public EarningsOuterApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetStandardResponse> GetStandard(string courseCode)
    {
        var response = await _httpClient.GetAsync($"{GetStandardUrl}/{courseCode}").ConfigureAwait(false);

        if (response.StatusCode.Equals(HttpStatusCode.NotFound))
            throw new Exception("Standard not found.");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Status code: {response.StatusCode} returned from earnings outer api.");

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var standard = JsonConvert.DeserializeObject<GetStandardResponse>(json);

        if (standard == null)
            throw new Exception("GetStandard returned null response from earnings outer api.");

        return standard;
    }
}