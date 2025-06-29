using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApi.Tests
{
    public class HelloApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient httpClient;

        public HelloApiTests(WebApplicationFactory<Program> factory)
        {
            httpClient = factory.CreateClient(); // Uses in-memory test server
        }

        [Fact]
        public async Task GetHello_ReturnsHelloWorld()
        {
            var response = await httpClient.GetAsync("/hello");
            var content = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Be("Hello, World!");
        }
    }
}