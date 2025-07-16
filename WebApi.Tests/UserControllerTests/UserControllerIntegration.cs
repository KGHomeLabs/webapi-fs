using FluentAssertions;

using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApi.Tests.Extensions;
using WebApi.Tests.Setup;

using WebApi.Tests.UserControllerTests.TestData;
using Xunit;

namespace WebApi.Tests.UserControllerTests
{
    [Collection("DatabaseCollection")]
    public class UserControllerIntegration : DatabaseIntegrationBase
    {
        private readonly HttpClient _client;

        public UserControllerIntegration(DataSetupFixture dbFixture, TestWebApplicationFactory factory)
            : base(dbFixture, factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "Me endpoint returns 200 with valid token")]
        [Trait("Integration Test", "Happy Path")]
        public async Task MeEndpoint_WithValidToken_ReturnsUserName()
        {
            var dataSetup = new UserControllerData(Connection);
            AddTestDataSetup(dataSetup);

            const string userId = "test001"; // From MeTestDataSetup.Seed
            _client.SetFakeJwtToken(
                new Claim("sub", userId),
                new Claim("iss", "test-issuer")
            );
            var response = await _client.GetAsync("api/user/me");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("test_user"); // Matches UserName from Seed

            dataSetup.Clean();
        }
    }
}
