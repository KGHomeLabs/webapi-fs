using Microsoft.AspNetCore.Builder;
using WebApi.Middleware;

namespace WebApi.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMyStuff(this IApplicationBuilder app)
        {
            return app.UseMiddleware<UserEnrichmentMiddleware>();
        }
    }
}
