using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace WebApi.Services
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' not found.");
        }

          public IDbConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            
            // Only set access token if using Azure AD authentication
            if (_connectionString.Contains("Authentication=Active Directory", StringComparison.OrdinalIgnoreCase))
            {
                // This requires the Azure.Identity NuGet package
                // But only gets called when the connection string indicates Azure AD auth
                try
                {
                    var credential = new Azure.Identity.DefaultAzureCredential();
                    var token = credential.GetToken(
                        new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }));
                    connection.AccessToken = token.Token;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle as needed
                    throw new InvalidOperationException("Failed to acquire Azure AD token for database connection", ex);
                }
            }
            
            return connection;
        }
    }
}
