using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using WebApi.Database;

public class DatabaseSetupService : IDatabaseSetupService
{
    private readonly string _connectionString;
    private readonly string _databaseName;

    public DatabaseSetupService(string connectionString)
    {
        _connectionString = connectionString;

        // Extract database name from connection string
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
        _databaseName = builder.InitialCatalog; 
    }

    public async Task EnsureDatabaseExistsAsync()
    {
        // Create connection string to master database
        var masterConnectionString = _connectionString.Replace($"Database={_databaseName}", "Database=master");

        using var connection = new SqlConnection(masterConnectionString);

        var databaseExists = await connection.QuerySingleOrDefaultAsync<int>(
            "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName",
            new { DatabaseName = _databaseName }
        );

        if (databaseExists == 0)
        {
            await connection.ExecuteAsync($"CREATE DATABASE [{_databaseName}]");
        }
    }

    public async Task EnsureTablesExistAsync()
    {
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);

        var createUsersTableSql = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
            BEGIN
                CREATE TABLE Users (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    UserId NVARCHAR(255) NOT NULL DEFAULT '',
                    IsAdmin BIT NOT NULL DEFAULT 0,
                    IsRoot BIT NOT NULL DEFAULT 0,
                    UserName NVARCHAR(255) NOT NULL DEFAULT '',
                    IsLockedOut BIT NOT NULL DEFAULT 0
                );
                
                -- Insert some test data
                INSERT INTO Users (UserId, IsAdmin, IsRoot, UserName, IsLockedOut) VALUES
                ('user001', 0, 0, 'john_doe', 0),
                ('admin001', 1, 0, 'admin_user', 0),
                ('root001', 1, 1, 'root_user', 0);
            END";

        await connection.ExecuteAsync(createUsersTableSql);
    }

    public async Task InitializeAsync()
    {
        await EnsureDatabaseExistsAsync();
        await EnsureTablesExistAsync();
    }
}