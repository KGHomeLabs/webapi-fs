
using FluentAssertions;
using System.Net;
using System.Threading.Tasks;
using System.Security.Claims;
using Moq;


namespace WebApi.Tests
{
    public class HelloApiTests
    {
        [Fact(DisplayName = "GET /hello with valid token returns user name")]
        public async Task GetHello_WithValidToken_ReturnsUserName()
        {
            // Arrange
            var fakeClaims = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim("sub", "123test")], "Test"));

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

        [Fact(DisplayName = "GET /hello without sub claim returns Unauthorized")]
        public async Task GetHello_WithoutSubClaim_ReturnsUnauthorized()
        {
            // Arrange - user with no sub claim
            var fakeClaims = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim("name", "John")], "Test"));

            var mockUserService = new Mock<IUserDataService>();
            var factory = new TestWebApplicationFactory(fakeClaims, mockUserService.Object);
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/hello");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "GET /hello with different roles")]
        public async Task GetHello_WithDifferentRoles_ReturnsCorrectly()
        {
            // Arrange
            var fakeClaims = new ClaimsPrincipal(
                new ClaimsIdentity([
                    new Claim("sub", "456admin"),
                    new Claim("role", "admin"),
                    new Claim("department", "IT")
                ], "Test"));

            var mockUserService = new Mock<IUserDataService>();
            mockUserService.Setup(x => x.GetUserDisplayName("456admin")).Returns("Alice Admin");

            var factory = new TestWebApplicationFactory(fakeClaims, mockUserService.Object);
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/hello");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Be("Hello, Alice Admin! (UserID: 456admin)");
        }
    }
}