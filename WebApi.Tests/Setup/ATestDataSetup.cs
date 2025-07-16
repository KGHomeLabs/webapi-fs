using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Tests.Setup
{
    public abstract class ATestDataSetup
    {
        protected readonly IDbConnection _connection;

        public ATestDataSetup(IDbConnection connection)
        {
            _connection = connection;
        }

        public virtual void Seed()
        {
            throw new NotImplementedException();
        }

        public virtual void Clean()
        {
            throw new NotImplementedException();
        }
    }
}
