using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using WebApi.Tests.Setup;

namespace WebApi.Tests.TestData
{
    public class UserControllerTestData : ATestDataSetup
    {
        public UserControllerTestData(IDbConnection connection) : base(connection)
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
