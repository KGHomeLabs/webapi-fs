using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace YourWebApi.Migrations
{
    public class Program
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
                if (!connectionString.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Non-azure database is targeted...");
                    await EnsureDatabaseExists(connectionString);
                } else
                {
                    Console.WriteLine("Azure database is targeted. Skipping EnsureDatabaseExists check.");
                }

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
            // Priority: Command line argument > Azure environment variable > appsettings.json

            // 1. Command line argument (for CI/CD or manual runs)
            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine("Using connection string from command line argument");
                return args[0];
            }

            // 2. Azure environment variable (for CI/CD targeting Azure)
            var azureConnectionString = Environment.GetEnvironmentVariable("AZURE_CONNECTION_STRING");
            if (!string.IsNullOrEmpty(azureConnectionString))
            {
                Console.WriteLine("Using Azure connection string from environment variable");
                return azureConnectionString;
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
            Console.WriteLine($"InitialCatalog is: {databaseName}");
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName";
            command.Parameters.AddWithValue("@DatabaseName", databaseName);

            var exists = (int) await command.ExecuteScalarAsync();
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