using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Serilog;
using Serilog.Events;

using OwlDocs.Data;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace OwlDocs.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                    .Build();

            Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();

                SeedDatabase(host);

                host.Run();

            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host Failed on Startup");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


        private static void SeedDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();

            var services = scope.ServiceProvider;

            var config = services.GetRequiredService<IConfiguration>();

            if (config["DocumentProvider"] == "Database")
            {
                var context = services.GetRequiredService<OwlDocsContext>();
                context.Database.EnsureCreated();
                DbInitializer.InitializeDatabase(context);
            }
        }
    }
}
