
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using WebApi.Extensions;


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
            var serviceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();



            // var jwtSettings = Configuration.GetSection("JwtSettings"); //TODO get back to this in production. currently this is environment safe
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddControllers();
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            // Add database setup service
            services.AddSingleton<DatabaseSetupService>(provider =>
            {                
                return new DatabaseSetupService(connectionString);
            });

            services.AddSingleton<IUserDataService, UserDataService>();
                        
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options => JWTOptionSelector.GetJwtBearerOptions(options, env));
            
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Initialize database
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var dbSetup = scope.ServiceProvider.GetRequiredService<DatabaseSetupService>();
                    await dbSetup.InitializeAsync();
                }
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