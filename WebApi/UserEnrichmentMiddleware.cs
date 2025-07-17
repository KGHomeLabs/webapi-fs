using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApi.Database.Models;
using WebApi.Services;

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

            if (context.User?.Identity?.IsAuthenticated != true)
            {
                context.User?.AddIdentity(new ClaimsIdentity());
                await _next(context);
                return;
            }

            var userId = context.User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing 'sub' claim");
                return;
            }

            var user = await userDataService.GetUserById(userId);
            if (user == null)
            {
                // Create new user if not found
                user = new UserDBO
                {
                    UserId = userId,
                    UserName = context.User.FindFirst("name")?.Value ?? userId,
                    IsAdmin = false,
                    IsRoot = false,
                    IsLockedOut = false
                };
                await userDataService.CreateUser(user);
            }

            if (user.IsLockedOut)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("User is locked out");
                return;
            }
            
            context.Items["UserDBO"] = user;
            await _next(context);
        }
    }
}
