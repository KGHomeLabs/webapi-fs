using FluentAssertions;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApi.Tests.Extensions;
using WebApi.Tests.Setup;
using WebApi.Tests.TestData;
using Xunit;

namespace WebApi.Tests
{
    [Collection("DatabaseCollection")]
    public class UserControllerIntegrationTests : DatabaseIntegrationBase
    {
        private readonly HttpClient _client;

        public UserControllerIntegrationTests(DataSetupFixture dbFixture, TestWebApplicationFactory factory)
            : base(dbFixture, factory)
        {
            _client = factory.CreateClient();
        }

        protected override ATestDataSetup CreateDataSetup(IDbConnection connection)
        {
            return new UserControllerTestData(connection);
        }

        [Fact(DisplayName = "Me endpoint returns 200 with valid token")]
        [Trait("Integration Test", "Happy Path")]
        public async Task MeEndpoint_WithValidToken_ReturnsUserName()
        {
            const string userId = "test001"; // From MeTestDataSetup.Seed
            _client.SetFakeJwtToken(
                new Claim("sub", userId),
                new Claim("iss", "test-issuer")
            );
            var response = await _client.GetAsync("/me");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("test_user"); // Matches UserName from Seed
        }
    }
}
