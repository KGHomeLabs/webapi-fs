
using FluentAssertions;
using System.Net;
using System.Threading.Tasks;
using System.Security.Claims;
using Moq;

namespace WebApi.Tests
{
    public class HelloApiTests
    {
        [Fact(DisplayName = "GET /hello without token returns Unauthorized 403")]
        public async Task GetHello_WithValidToken_ReturnsUserName()
        {
            // Arrange
            var fakeClaims = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim("sub", "123test")], "mock"));

            var mockUserService = new Mock<IUserDataService>();
            mockUserService.Setup(x => x.GetUserDisplayName("123test")).Returns("Bob");

            var factory = new TestWebApplicationFactory(fakeClaims, mockUserService.Object);
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/hello");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Be("Hello, Bob! (UserID: 123test)");
        }
    }
}