using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OwlDocs.Data;
using Microsoft.Extensions.DependencyInjection;

namespace OwlDocs.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            SeedDatabase(host);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
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
