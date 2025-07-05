using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Claims;



namespace WebApi.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program> // Use <Startup> if not using minimal API
    {
        private readonly IUserDataService _mockUserDataService;
        private readonly ClaimsPrincipal _user;

        public TestWebApplicationFactory(ClaimsPrincipal user, IUserDataService mockUserDataService)
        {
            _user = user;
            _mockUserDataService = mockUserDataService;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the original IUserDataService registration
                services.RemoveAll<IUserDataService>();
                // Add the mock IUserDataService
                services.AddSingleton(_mockUserDataService);

                // Add test authentication
                services.AddAuthentication(defaultScheme: "Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                // Register the test user
                services.AddSingleton(_user);
            });
        }
    }
}
