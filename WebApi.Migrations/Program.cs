using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace YourWebApi.Migrations
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                var configuration = BuildConfiguration();
                var connectionString = GetConnectionString(args, configuration);

                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("No connection string found!");
                    Console.WriteLine("Usage:");
                    Console.WriteLine("  Local: dotnet run (uses appsettings.json)");
                    Console.WriteLine("  CI/CD: dotnet run <connection-string>");
                    return 1;
                }

                var serviceProvider = CreateServices(connectionString);
                using var scope = serviceProvider.CreateScope();

                // Create database if it doesn't exist
                await EnsureDatabaseExists(connectionString);

                // Run migrations
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();

                Console.WriteLine("Migrations completed successfully!");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return 1;
            }
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static string GetConnectionString(string[] args, IConfiguration configuration)
        {
            // Priority: Command line argument > Environment variable > appsettings.json, a bit experimental but should work well on CICD

            // 1. Command line argument (for CI/CD)
            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine("Using connection string from command line argument");
                return args[0];
            }

            // 2. Environment variable (for CI/CD with env vars)
            var envConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
            if (!string.IsNullOrEmpty(envConnectionString))
            {
                Console.WriteLine("Using connection string from environment variable");
                return envConnectionString;
            }

            // 3. appsettings.json (for local development)
            var configConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(configConnectionString))
            {
                Console.WriteLine("Using connection string from appsettings.json");
                return configConnectionString;
            }

            return null;
        }

        private static ServiceProvider CreateServices(string connectionString)
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSqlServer()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(Program).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddConsole())
                .BuildServiceProvider(false);
        }

        private static async Task EnsureDatabaseExists(string connectionString)
        {
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;

            var masterConnectionString = connectionString.Replace($"Database={databaseName}", "Database=master");

            using var connection = new Microsoft.Data.SqlClient.SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName";
            command.Parameters.AddWithValue("@DatabaseName", databaseName);

            var exists = (int)await command.ExecuteScalarAsync();
            if (exists == 0)
            {
                command.CommandText = $"CREATE DATABASE [{databaseName}]";
                command.Parameters.Clear();
                await command.ExecuteNonQueryAsync();
                Console.WriteLine($"Created database: {databaseName}");
            }
        }
    }
}