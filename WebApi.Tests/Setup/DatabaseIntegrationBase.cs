using System;
using System.Data;
using Xunit;

namespace WebApi.Tests.Setup
{
    public abstract class DatabaseIntegrationBase : IClassFixture<TestWebApplicationFactory>, IDisposable
    {
        protected readonly IDbConnection Connection;
        protected readonly ATestDataSetup DataSetup;

        protected DatabaseIntegrationBase(DataSetupFixture dbFixture, TestWebApplicationFactory factory)
        {
            Connection = dbFixture.Connection;
            DataSetup = CreateDataSetup(Connection);
            DataSetup.Seed();
        }

        protected abstract ATestDataSetup CreateDataSetup(IDbConnection connection);

        public virtual void Dispose()
        {
            DataSetup.Clean();
        }
    }
}
