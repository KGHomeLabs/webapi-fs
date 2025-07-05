using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Middleware
{
    public class UserEnrichmentMiddleware
    {
        private readonly RequestDelegate _next;

        public UserEnrichmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserDataService userDataService)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Ensure context.User.Identity is not null before accessing IsAuthenticated
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                // If the user is not authenticated, we don't enrich the claims
                context.User?.AddIdentity(new ClaimsIdentity());
            }            
            else
            {
                var userId = context.User.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var displayName = userDataService.GetUserDisplayName(userId);
                    var claimsIdentity = new ClaimsIdentity(new[] { new Claim("userFart", displayName) }, "UserRepo");
                    context.User.AddIdentity(claimsIdentity);
                }
            }
            await _next(context);
        }
    }
}
