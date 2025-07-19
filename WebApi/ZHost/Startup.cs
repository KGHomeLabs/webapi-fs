using CorrelationId;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using WebApi.Services;
using WebApi.ZHost.Config;
using WebApi.ZHost.Middleware;


namespace WebApi.ZHost
{
    public class Startup
    {     
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            // var jwtSettings = Configuration.GetSection("JwtSettings"); //TODO get back to this in production. currently this is environment safe
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddDefaultCorrelationId(options =>
            {
                options.AddToLoggingScope = true;
                options.EnforceHeader = false;
                options.IncludeInResponse = true;
            });
            services.AddControllers();

            services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
            services.AddSingleton<IUserDataService, UserDataService>();
                        
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options => JWTOptionSelector.GetJwtBearerOptions(options, Environment));
            
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCorrelationId();
            app.UseSerilogRequestLogging();
            if (env.IsDevelopment())
            {               
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();  
            app.UseUserRepositoryClaims(); // My middleware adds a prototype comical userFart claim from my UserDataService
            app.UseAuthorization();  

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}