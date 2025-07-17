using System.Threading.Tasks;

namespace WebApi.Database
{
    public interface IDatabaseSetupService
    {
        Task EnsureTablesExistAsync();
    }
}
