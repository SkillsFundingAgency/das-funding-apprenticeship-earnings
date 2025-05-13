using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Health;

public abstract class BaseHealthCheck<T> : IHealthCheck
{
    private HealthCheckResult _cachedResult;
    private DateTimeOffset _lastCheckTime = DateTimeOffset.MinValue;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);
    private readonly object _lock = new();
    private readonly ILogger<T> _logger;

    protected BaseHealthCheck(ILogger<T> logger)
    {
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var cachedResult = GetCachedResult();
        if (cachedResult != null)
        {
            return cachedResult.Value;
        }

        var result = await HealthCheck(context, cancellationToken);

        SetCachedResult(result);
        return result;
    }

    public abstract Task<HealthCheckResult> HealthCheck(HealthCheckContext context, CancellationToken cancellationToken);

    protected void LogError(string error, Exception ex)
    {
        _logger.LogError($"{error} Exception:{ex.Message}", ex);
    }

    private HealthCheckResult? GetCachedResult()
    {
        lock (_lock)
        {
            if (DateTimeOffset.UtcNow - _lastCheckTime < _cacheDuration)
            {
                return _cachedResult;
            }
        }

        return null;
    }

    private void SetCachedResult(HealthCheckResult result)
    {
        lock (_lock)
        {
            _cachedResult = result;
            _lastCheckTime = DateTimeOffset.UtcNow;
        }
    }

}