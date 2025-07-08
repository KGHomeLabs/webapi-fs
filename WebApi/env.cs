using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Reflection.Metadata.Ecma335;

namespace WebApi
{
   public static class EnvironmentExtension
    {
        public static bool  IsDev(this IWebHostEnvironment env)
        {
            return env.IsDevelopment() || env.EnvironmentName == "Development" || env.EnvironmentName == "Local";
        }

        public static bool IsProd(this IWebHostEnvironment env)
        {
            return env.IsProduction() || env.EnvironmentName.ToLower() == "Prod".ToLower();
        }

        public static bool IsPreview(this IWebHostEnvironment env)
        {
            return env.IsStaging() || env.EnvironmentName.ToLower() == "Prev".ToLower();
        }
    }
}
