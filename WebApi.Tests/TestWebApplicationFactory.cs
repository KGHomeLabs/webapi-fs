using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WebApi.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
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
                // REMOVE the original registration (from Startup)
                services.RemoveAll<IUserDataService>();

                // ADD your mock
                services.AddSingleton(_mockUserDataService);

                // OPTIONAL: inject claims principal via custom IHttpContextAccessor
                services.AddSingleton<IHttpContextAccessor>(new FakeHttpContextAccessor(_user));
            });
        }
    }

}
