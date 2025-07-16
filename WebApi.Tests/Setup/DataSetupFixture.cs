using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace WebApi.Tests.Setup
{
    public class DataSetupFixture : IDisposable
    {
        public IDbConnection Connection { get; }

        public DataSetupFixture()
        {
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Run schema and operational migrations
            var currentDir = Directory.GetCurrentDirectory();
            var solutionDir = Directory.GetParent(currentDir)?.Parent?.Parent?.Parent?.FullName;
            var migrationsDir = Path.Combine(solutionDir, "WebApi.Migrations");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project WebApi.Migrations.csproj",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = migrationsDir
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Debug.WriteLine($"Migration Output: {output}");
            if (process.ExitCode != 0)
            {
                Debug.WriteLine($"Migration Error: {error}");
                throw new System.Exception($"Migration failed: {error}");
            }

            // Create and open single connection
            Connection = new SqlConnection(connectionString);
            Connection.Open();
        }

        public void Dispose()
        {
            Connection?.Close();
            Connection?.Dispose();
        }
    }

    [CollectionDefinition("DatabaseCollection")]
    public class DatabaseCollection : ICollectionFixture<DataSetupFixture>
    {
        // This class has no code; it defines the collection for xUnit
    }
}
