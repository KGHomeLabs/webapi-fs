using System.Data;

namespace WebApi.Services
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
