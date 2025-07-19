using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;

namespace WebApi.Services
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly ILogger<SqlConnectionFactory> _logger;
        private readonly string _connectionString;
        public SqlConnectionFactory(IConfiguration configuration, ILogger<SqlConnectionFactory> logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("[ConnectionFactory] Connection string 'DefaultConnection' null or empty.");
            }
        }

          public IDbConnection CreateConnection()
        {
            _logger.LogInformation("Creating SQL connection using configured connection string.");

            var connection = new SqlConnection(_connectionString);

            if (_connectionString.Contains("Authentication=Active Directory", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Detected Azure AD authentication in connection string. Acquiring access token...");

                try
                {
                    _logger.LogInformation("Removed block");
                    //var credential = new DefaultAzureCredential();
                    //var tokenRequestContext = new TokenRequestContext(new[] { "https://database.windows.net/.default" });

                    //var token = credential.GetToken(tokenRequestContext);

                    //_logger.LogInformation("Successfully acquired Azure AD token. Setting AccessToken on SqlConnection.");
                    //connection.AccessToken = token.Token;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to acquire Azure AD token for SQL connection.");
                    throw new InvalidOperationException("Failed to acquire Azure AD token for database connection", ex);
                }
            }

            _logger.LogInformation("SQL connection instance created successfully (not yet opened).");
            return connection;
        }
    }
}
