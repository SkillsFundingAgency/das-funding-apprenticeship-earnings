using Microsoft.Data.SqlClient;
using System.Data;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

public class SqlDatabase : IDisposable
{
    private bool _isDisposed;

#if !DEBUG
    private bool _useLocalDb = true;
    private const string _source = "(localdb)\\MSSQLLocalDB";
    private const string _authentication = "Integrated Security=True";
#else
    private bool _useLocalDb = false;
    private const string _source = @"127.0.0.1\sql_server_container,1433";
    private const string _authentication = "User Id=sa;Password=P1peline;TrustServerCertificate=True";
#endif

    public DatabaseInfo DatabaseInfo { get; } = new DatabaseInfo();

    public SqlDatabase(string? dbName = null)
    {
        DatabaseInfo.SetDatabaseName(dbName ?? Guid.NewGuid().ToString());
        CreateTestDatabase();
    }

    private void CreateTestDatabase()
    {
        Directory.CreateDirectory("C:\\temp");
        var dbConnectionString = @$"Data Source={_source};Initial Catalog={DatabaseInfo.DatabaseName};{_authentication};MultipleActiveResultSets=True;Pooling=False;Connect Timeout=30;";
        var masterConnectionString = @$"Data Source={_source};Initial Catalog=master;{_authentication};MultipleActiveResultSets=true";

        DatabaseInfo.SetConnectionString(dbConnectionString);

        using var dbConn = new SqlConnection(masterConnectionString);
        try
        {
            var sql = "";
            if (_useLocalDb)
            {
                sql = @$"CREATE DATABASE [{DatabaseInfo.DatabaseName}] ON PRIMARY
                     (NAME = [{DatabaseInfo.DatabaseName}_Data],
                      FILENAME = 'C:\\temp\\{DatabaseInfo.DatabaseName}.mdf')
                      LOG ON (NAME = [{DatabaseInfo.DatabaseName}_Log],
                      FILENAME = 'C:\\temp\\{DatabaseInfo.DatabaseName}.ldf')";
            }
            else
            {
                sql = @$"CREATE DATABASE [{DatabaseInfo.DatabaseName}]";
            }


            using var cmd = new SqlCommand(sql, dbConn);
            dbConn.Open();
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{nameof(SqlDatabaseModel)}] {nameof(CreateTestDatabase)} failed. Exception={ex}");
        }
        finally
        {
            if (dbConn.State == ConnectionState.Open)
            {
                dbConn.Close();
            }
        }

        using var dbConnection = new SqlConnection(DatabaseInfo.ConnectionString);
        dbConnection.Open();
    }

    private void DeleteTestDatabase()
    {
        try
        {
            var files = new List<string>();
            using var dbConn = new SqlConnection(DatabaseInfo.ConnectionString);
            using var cmd = new SqlCommand("SELECT DB_NAME()", dbConn);
            dbConn.Open();
            var dbName = cmd.ExecuteScalar();
            cmd.CommandText = "SELECT filename FROM sysfiles";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    files.Add((string)reader["filename"]);
                }
            }
            cmd.CommandText = $"ALTER DATABASE [{dbName}] SET OFFLINE WITH ROLLBACK IMMEDIATE";
            cmd.ExecuteNonQuery();
            cmd.CommandText = $"EXEC sp_detach_db '{dbName}', 'true';";
            cmd.ExecuteNonQuery();
            dbConn.Close();

            files.ForEach(DeleteFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{nameof(SqlDatabase)}] {nameof(DeleteTestDatabase)} exception thrown: {ex.Message}");
            throw ex;
        }
    }

    private static void DeleteFile(string file)
    {
        try
        {
            File.Delete(file);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{nameof(SqlDatabase)}] {nameof(DeleteFile)} exception thrown: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            DeleteTestDatabase();
        }

        _isDisposed = true;
    }
}