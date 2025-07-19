
// TestWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApi.Services;
using WebApi.ZHost;

namespace WebApi.Tests.Setup
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the original IUserDataService registration
                services.RemoveAll<IUserDataService>();

                // Add the mock IUserDataService
                services.AddSingleton<IUserDataService, UserDataService>();
            });
        }
    }
}