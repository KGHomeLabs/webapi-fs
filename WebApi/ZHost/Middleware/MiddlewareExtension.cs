using Microsoft.AspNetCore.Builder;

namespace WebApi.ZHost.Middleware
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUserRepositoryClaims(this IApplicationBuilder app)
        {
            return app.UseMiddleware<UserEnrichmentMiddleware>();
        }
    }
}
