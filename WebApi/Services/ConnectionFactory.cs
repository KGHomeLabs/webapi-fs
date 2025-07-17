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
            return new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        }
    }
}
