using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Database.Models;

namespace WebApi.Services
{
    public interface IUserDataService
    {
        Task<string> GetUserDisplayName(string userId);
        Task<UserDBO> GetUserById(string userId);
        Task<IEnumerable<UserDBO>> GetAllUsers();
        Task CreateUser(UserDBO user);
        Task UpdateUser(string userId, UserDBO user);
        Task DeleteUser(string userId);
    }
}