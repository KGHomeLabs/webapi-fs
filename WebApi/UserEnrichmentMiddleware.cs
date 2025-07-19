using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;
        
        public UserEnrichmentMiddleware(RequestDelegate next, ILogger<UserEnrichmentMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserDataService userDataService)
        {
            if (context == null)
            {
                var ex = new ArgumentNullException(nameof(context));
                _logger.LogError(ex, "HttpContext was null in UserEnrichmentMiddleware.");
                throw ex;
            }

            try
            {
                if (context.User?.Identity?.IsAuthenticated != true)
                {
                    _logger.LogDebug("Unauthenticated request. Injecting empty identity.");
                    context.User?.AddIdentity(new ClaimsIdentity());
                    await _next(context);
                    return;
                }

                var userId = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Authenticated request missing 'sub' claim. Rejecting with 401.");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Missing 'sub' claim");
                    return;
                }

                _logger.LogDebug("Fetching user with ID: {UserId}", userId);
                var user = await userDataService.GetUserById(userId);

                if (user == null)
                {
                    _logger.LogInformation("User with ID {UserId} not found. Creating new user.", userId);
                    user = new UserDBO
                    {
                        UserId = userId,
                        UserName = context.User.FindFirst("name")?.Value ?? userId,
                        IsAdmin = false,
                        IsRoot = false,
                        IsLockedOut = false
                    };

                    await userDataService.CreateUser(user);
                    _logger.LogInformation("New user created: {UserName}", user.UserName);
                }

                if (user.IsLockedOut)
                {
                    _logger.LogWarning("User {UserId} is locked out. Rejecting with 403.", userId);
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("User is locked out");
                    return;
                }

                context.Items["UserDBO"] = user;
                _logger.LogDebug("User {UserId} added to HttpContext.Items", userId);
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in UserEnrichmentMiddleware");
                throw;
            }
        }
    }
}
