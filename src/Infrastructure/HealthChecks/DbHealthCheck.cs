using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.HealthChecks;

[ExcludeFromCodeCoverage]
public class DbHealthCheck : BaseHealthCheck<DbHealthCheck>
{
    private readonly string _connectionString;
    private readonly ISqlAzureIdentityTokenProvider? _tokenProvider;

    public DbHealthCheck(string connectionString, ILogger<DbHealthCheck> logger, ISqlAzureIdentityTokenProvider? tokenProvider) : base(logger)
    {
        _connectionString = connectionString;
        _tokenProvider = tokenProvider;
    }

    public override async Task<HealthCheckResult> HealthCheck(CancellationToken cancellationToken)
    {
        try
        {
            
            using var connection = new SqlConnection(_connectionString);

            if (_tokenProvider != null) 
            {
                var token = await _tokenProvider.GetAccessTokenAsync();
                connection.AccessToken = token;
            }
            

            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("Database connection is OK.");
        }
        catch (Exception ex)
        {
            LogError("Database connection failed.", ex);
            return HealthCheckResult.Unhealthy("Database connection failed.");
        }
    }
}

