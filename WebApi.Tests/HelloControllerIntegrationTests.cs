using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using System.Net;
using Xunit;
using System;
using System.Diagnostics;
using WebApi.Tests.Extensions;

namespace WebApi.Tests
{
    public class HelloControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public HelloControllerIntegrationTests(TestWebApplicationFactory factory)
        {
            // Force console to actually output to the debug window
            Console.SetOut(new System.IO.StringWriter());
            Console.SetError(new System.IO.StringWriter());

            // Now use Debug.WriteLine instead - this WILL show up
            System.Diagnostics.Debug.WriteLine("=== TEST CONSTRUCTOR START ===");
            Console.WriteLine(Console.Out.NewLine + "HelloControllerIntegrationTests constructor called #################################");
            _client = factory.CreateClient();
        }


        [Fact(DisplayName = "Returns 200 with token provided")]
        [Trait("Integration Test", "Happy Path")]
        public async Task HelloEndpoint_WithValidToken_ReturnsExpectedResponse()
        {
            const string userId = "test-user-123";

            _client.SetFakeJwtToken(
                new Claim("sub", userId),
                new Claim("iss", "test-issuer")
            );
            var response = await _client.GetAsync("/hello");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain($"{userId}");
        }

        [Fact(DisplayName = "Returns 401 Unauthorized when no JWT token is provided")]
        [Trait("Integration Test", "Authentication")]
        public async Task HelloEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/hello");
            //should be unauthorized because there is no token set in this case.
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }


        [Fact(DisplayName = "Returns 401 Unauthorized when the JWT does not contain required claims")]
        [Trait("Integration Test", "Authentication")]
        public async Task HelloEndpoint_CallsServiceCorrectlyMissingClaims()
        {
            _client.SetFakeJwtToken(new Claim("iss", "test-issuer"));

            try
            {
                var response = await _client.GetAsync("/hello");
                //should be unauthorized because there is no token set in this case.
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FULL STACK TRACE: {ex.StackTrace}");
                throw; // Re-throw so the test fails
            }
        }
    }
}