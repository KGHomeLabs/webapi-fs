using Microsoft.Extensions.Logging;
using SqlKata;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WebApi.Database.Models;

namespace WebApi.Services
{
    public class UserDataService : IUserDataService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<UserDataService> _logger;
        public UserDataService(IDbConnectionFactory connectionFactory, ILogger<UserDataService> logger)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        private QueryFactory CreateQueryFactory()
        {
            var connection = _connectionFactory.CreateConnection();
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    _logger.LogInformation("Attempting to open database connection...");
                    connection.Open();
                    _logger.LogInformation("Database connection opened successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open database connection.");
                throw;
            }

            return new QueryFactory(connection, new SqlKata.Compilers.SqlServerCompiler());
        }

        public async Task<string> GetUserDisplayName(string userId)
        {
            using var queryFactory = CreateQueryFactory();
            var query = new Query("Users").Select("UserName").Where("UserId", userId);

            try
            {
                var userName = await queryFactory.FirstOrDefaultAsync<string>(query);
                return userName ?? $"UnknownUser_{userId}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying user display name for {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDBO> GetUserById(string userId)
        {
            using var queryFactory = CreateQueryFactory();
            var query = new Query("Users").Where("UserId", userId);

            try
            {
                return await queryFactory.FirstOrDefaultAsync<UserDBO>(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying user by ID {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<UserDBO>> GetAllUsers(int page = 1, int pageSize = 10)
        {
            using var queryFactory = CreateQueryFactory();
            var query = new Query("Users")
                .Offset((page - 1) * pageSize)
                .Limit(pageSize);

            try
            {
                return await queryFactory.GetAsync<UserDBO>(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users (page {Page}, size {PageSize})", page, pageSize);
                throw;
            }
        }

        public async Task CreateUser(UserDBO user)
        {
            using var queryFactory = CreateQueryFactory();
            var query = new Query("Users").AsInsert(new
            {
                user.UserId,
                user.IsAdmin,
                user.IsRoot,
                user.UserName,
                user.IsLockedOut
            });

            try
            {
                await queryFactory.ExecuteAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {UserId}", user.UserId);
                throw;
            }
        }

        public async Task UpdateUser(string userId, UserDBO user)
        {
            using var queryFactory = CreateQueryFactory();
            var query = new Query("Users").Where("UserId", userId).AsUpdate(new
            {
                user.IsAdmin,
                user.IsRoot,
                user.UserName,
                user.IsLockedOut
            });

            try
            {
                await queryFactory.ExecuteAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                throw;
            }
        }

        public async Task DeleteUser(string userId)
        {
            using var queryFactory = CreateQueryFactory();
            var query = new Query("Users").Where("UserId", userId).AsDelete();

            try
            {
                await queryFactory.ExecuteAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                throw;
            }
        }
    }
}