using Microsoft.AspNetCore.Connections;
using Dapper;
using SqlKata;
using SqlKata.Execution;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Database.Models;

namespace WebApi.Services
{
    public class UserDataService : IUserDataService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserDataService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private QueryFactory CreateQueryFactory()
        {
            var connection = _connectionFactory.CreateConnection();
            return new QueryFactory(connection, new SqlKata.Compilers.SqlServerCompiler());
        }

        public async Task<string> GetUserDisplayName(string userId)
        {
            using var queryFactory = CreateQueryFactory();
            var query = new Query("Users").Select("UserName").Where("UserId", userId);
            var userName = await queryFactory.FirstOrDefaultAsync<string>(query);
            return userName ?? $"UnknownUser_{userId}";
        }

        public async Task<UserDBO> GetUserById(string userId)
        {
            using var queryFactory = CreateQueryFactory();
            var query = new Query("Users").Where("UserId", userId);
            return await queryFactory.FirstOrDefaultAsync<UserDBO>(query);
        }

        public async Task<IEnumerable<UserDBO>> GetAllUsers()
        {
            using var queryFactory = CreateQueryFactory();
            var query = new Query("Users");
            return await queryFactory.GetAsync<UserDBO>(query);
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
            await queryFactory.ExecuteAsync(query);
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
            await queryFactory.ExecuteAsync(query);
        }

        public async Task DeleteUser(string userId)
        {
            using var queryFactory = CreateQueryFactory();
            var query = new Query("Users").Where("UserId", userId).AsDelete();
            await queryFactory.ExecuteAsync(query);
        }
    }
}