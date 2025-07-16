using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.Tests.Setup;

namespace WebApi.Tests.UserControllerTests.TestData
{
    public class AdminUserData : ATestDataSetup
    {
        public AdminUserData(IDbConnection connection) : base(connection)
        {
        }

        public override void Seed()
        {
            var db = new QueryFactory(_connection, new SqlServerCompiler());
            db.Query("Users").Insert(new
            {
                UserId = "admin001",
                IsAdmin = true,
                IsRoot = false,
                UserName = "admin_user",
                IsLockedOut = false
            });
            db.Query("Users").Insert(new
            {
                UserId = "user001",
                IsAdmin = false,
                IsRoot = false,
                UserName = "regular_user",
                IsLockedOut = false
            });
        }

        public override void Clean()
        {
            var db = new QueryFactory(_connection, new SqlServerCompiler());
            db.Query("Users").WhereIn("UserId", new[] { "admin001", "user001" }).Delete();
        }
    }
}
