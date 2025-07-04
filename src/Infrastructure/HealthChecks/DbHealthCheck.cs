using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.HealthChecks;

[ExcludeFromCodeCoverage]
public class DbHealthCheck : BaseHealthCheck<DbHealthCheck>
{
    private readonly string _connectionString;

    public DbHealthCheck(string connectionString, ILogger<DbHealthCheck> logger) : base(logger)
    {
        _connectionString = connectionString;
    }

    public override async Task<HealthCheckResult> HealthCheck(CancellationToken cancellationToken)
    {
        try
        {
            
            using var connection = new SqlConnection(_connectionString);
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

