using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using WebApi.Tests.Setup;

namespace WebApi.Tests.UserControllerTests.TestData
{
    public class UserNonAdminData : ATestDataSetup
    {      

        public UserNonAdminData(IDbConnection connection) : base(connection)
        {
        }

        public override void Seed()
        {
            var db = new QueryFactory(_connection, new SqlServerCompiler());
            db.Query("Users").Insert(new
            {
                UserId = "test001",
                IsAdmin = false,
                IsRoot = false,
                UserName = "test_user",
                IsLockedOut = false
            });
        }

        public override void Clean()
        {
            var db = new QueryFactory(_connection, new SqlServerCompiler());
            db.Query("Users").Where("UserId", "test001").Delete();
        }
    }
}
