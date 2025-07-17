using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace WebApi.Tests.Setup
{
    public abstract class DatabaseIntegrationBase : IClassFixture<TestWebApplicationFactory>, IDisposable
    {
        protected readonly IDbConnection Connection;
        protected readonly List<ATestDataSetup> TestDataSetups;

        protected DatabaseIntegrationBase(DataSetupFixture dbFixture, TestWebApplicationFactory factory)
        {
            Connection = dbFixture.Connection;
            TestDataSetups = new List<ATestDataSetup>();
        }

        protected void AddTestDataSetup(ATestDataSetup setup)
        {
            TestDataSetups.Add(setup);
            setup.Seed();
        }

        public virtual void Dispose()
        {
            foreach (var setup in TestDataSetups)
            {
                setup.Clean();
            }
        }
    }
}
