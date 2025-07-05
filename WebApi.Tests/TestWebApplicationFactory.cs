
// TestWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using WebApi;
using WebApi.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public UserDataServiceMock TestUserDataService { get; }

    public TestWebApplicationFactory()
    {
        // Create the concrete test implementation
        TestUserDataService = new UserDataServiceMock();
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the original IUserDataService registration
            services.RemoveAll<IUserDataService>();

            // Add the mock IUserDataService
            services.AddSingleton<IUserDataService>(TestUserDataService);
        });
    }
}
