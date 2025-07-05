using Microsoft.AspNetCore.Authentication;

using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace WebApi.Tests
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ClaimsPrincipal _user;

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
               LoggerFactory logger, UrlEncoder encoder, ISystemClock clock, ClaimsPrincipal user)
            : base(options, logger, encoder, clock)
        {
            _user = user;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var ticket = new AuthenticationTicket(_user, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
