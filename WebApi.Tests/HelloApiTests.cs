using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
// Adjust the namespace according to your project structure
using Xunit;

namespace WebApi.Tests;

public class HelloApiTests : IClassFixture<WebApplicationFactory<WebApi.Program>>
{
    private readonly HttpClient _client;

    public HelloApiTests(WebApplicationFactory<WebApi.Program> factory)
    {
        _client = factory.CreateClient(); // Uses in-memory test server
    }

    [Fact]
    public async Task GetHello_ReturnsHelloWorld()
    {
        var response = await _client.GetAsync("/hello");
        var content = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal("Hello, World!", content);
    }
}