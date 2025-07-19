using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.WithCaller;
using Serilog.Events;
using System;
using System.Diagnostics;

namespace WebApi
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Console.Title = "Pokemon Webservice";
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var isAzure = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") != null;

            var loggerConfigBuilder = new ConfigurationBuilder()
                                    .AddJsonFile("appsettings.json", optional: true)
                                    .AddJsonFile($"appsettings.{environment}.json", optional: true)
                                    .AddEnvironmentVariables()
                                    .Build();

            var loggerConfig = new LoggerConfiguration()
                 .MinimumLevel.Debug()
                 .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                 .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                 .MinimumLevel.Override("System", LogEventLevel.Warning)
                 .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                 .ReadFrom.Configuration(loggerConfigBuilder) // load Serilog from appsettings.json
                 .Enrich.FromLogContext()
              //   .Enrich.With<SerilogEnricher>()
                 .Enrich.WithCorrelationId() // <--- this line is the key addition
                 .Enrich.WithProperty("Application", "WebApi");

            if (isAzure)
            {
                loggerConfig.WriteTo.File(
                    @"D:\home\LogFiles\Application\myapp.txt",
                    fileSizeLimitBytes: 1_000_000,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1));
            }
            Log.Logger = loggerConfig.CreateLogger();

            try
            {
                Log.Information("Starting host...");
                Log.Information("ENV: {Env}, Azure: {Azure}", environment, isAzure);
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog() // use the loaded Serilog config
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}