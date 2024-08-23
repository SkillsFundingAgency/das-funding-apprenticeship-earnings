﻿using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api;

public class ApiClient<T> : IApiClient<T> where T : class, IApiConfig, new()
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient, IOptions<T> config)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(config.Value.BaseUrl);
    }

    public async Task<ApiResponse<TResponse>> Get<TResponse>(IGetApiRequest request)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);

        var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

        return await ProcessResponse<TResponse>(response);
    }

    private static async Task<ApiResponse<TResponse>> ProcessResponse<TResponse>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var errorContent = "";
        var responseBody = default(TResponse?);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized) throw new ApiUnauthorizedException();

            errorContent = json;
        }
        else if (!string.IsNullOrWhiteSpace(json))
        {
            responseBody = JsonSerializer.Deserialize<TResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        responseBody = JsonSerializer.Deserialize<TResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var apiResponse = new ApiResponse<TResponse>(responseBody, response.StatusCode, errorContent);

        return apiResponse;
    }
}

public class ApiUnauthorizedException : Exception { }