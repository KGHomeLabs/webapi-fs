using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //     services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
            //     {
            //         options.Authority = "https://your-auth-provider/";
            //         options.Audience = "your-api";
            //         options.RequireHttpsMetadata = false;

            //         // Optional debugging
            //         options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            //         {
            //             OnTokenValidated = ctx =>
            //             {
            //                 // inspect ctx.Principal here if needed
            //                 return Task.CompletedTask;
            //             }
            //         };
            //     }
            // );

            services.AddAuthorization();
            services.AddSingleton<IUserDataService, UserDataService>(); // REAL registration
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}