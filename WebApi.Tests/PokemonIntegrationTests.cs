using Moq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using System.Net;
using Xunit;
using System;
using System.Diagnostics;

namespace WebApi.Tests;
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

    [Fact]
    public void SimpleTest()
    {
        var x = 5; // SET BREAKPOINT HERE
        var y = 10;
        Assert.Equal(15, x + y);
    }

    [Fact]
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
        content.Should().Contain($"TestRealUser_{userId}");   
    }

    [Fact]
    public async Task HelloEndpoint_WithoutToken_ReturnsUnauthorized()
    {      
        var response = await _client.GetAsync("/hello");
        //should be unauthorized because there is no token set in this case.
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HelloEndpoint_WithDifferentUser_CallsServiceCorrectly()
    {
        _client.SetFakeJwtToken(new Claim("sub", "another-user"));

        try
        {
            var response = await _client.GetAsync("/hello");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Hello, Jane Smith!").And.Contain("UserID: another-user");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FULL STACK TRACE: {ex.StackTrace}");
            throw; // Re-throw so the test fails
        }
    }
}