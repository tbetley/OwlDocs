using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;

using Markdig;

using Serilog;
using Serilog.Events;

using OwlDocs.Data;
using OwlDocs.Web.Options;
using OwlDocs.Models.Options;
using OwlDocs.Domain.DocumentService;
using OwlDocs.Domain.DocumentCache;
using OwlDocs.Web.Authorization;
using OwlDocs.Data.Repositories;
using System.Runtime.InteropServices;

namespace OwlDocs.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {   
            // Set up early logging to catch if Host fails to build/run
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
                var builder = WebApplication.CreateBuilder(args);

                // Add Options 
                builder.Services.AddOptions<AuthOptions>().Bind(configuration.GetSection(AuthOptions.AuthSettings));
                builder.Services.AddOptions<DocumentOptions>().Bind(configuration.GetSection(DocumentOptions.DocumentSettings));
                builder.Services.AddOptions<SiteOptions>().Bind(configuration.GetSection(SiteOptions.SiteSettings));

                // Add 3rd party services
                builder.Services.AddSingleton(md => new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
                                
                var docProvider = configuration["DocumentSettings:Provider"];
                var database = configuration["DocumentSettings:Database"];
                if (docProvider == DocumentOptions.Database)
                {
                    if (database == DocumentOptions.Sqlite)
                    {
                        builder.Services.AddScoped<ISqliteRepository>(db => new SqliteRepository(configuration.GetConnectionString("Sqlite")));
                    }
                    
                    builder.Services.AddScoped<IDocumentService, DbDocumentService>();
                }
                else if (docProvider == DocumentOptions.File)
                {
                    // add file services
                    builder.Services.AddScoped<IDocumentService, FileDocumentService>();
                }

                builder.Services.AddSingleton<IDocumentCache, DocumentCache>();

                if (configuration.GetValue<string>("Authoization:Type") == AuthorizationType.ActiveDirectory.ToString("D"))
                {
                    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate(options =>
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            options.EnableLdap(settings =>
                            {
                                settings.Domain = "ad.betley.io";                                
                            });
                        }
                    });
                }

                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy(Policies.DocumentReadersPolicy,
                        policy => policy.Requirements.Add(new DocumentReadersRequirement()));

                    options.AddPolicy(Policies.DocumentWritersPolicy,
                        policy => policy.Requirements.Add(new DocumentWritersRequirement()));

                    options.AddPolicy(Policies.SiteAdminsPolicy,
                        policy => policy.Requirements.Add(new SiteAdminsRequirement()));

                });

                builder.Services.AddSingleton<IAuthorizationHandler, DocumentReadersHandler>();
                builder.Services.AddSingleton<IAuthorizationHandler, DocumentWritersHandler>();
                builder.Services.AddSingleton<IAuthorizationHandler, SiteAdminsHandler>();

                // Add asp.net core required services
                builder.Services.AddMvc();

                var app = builder.Build();
                SeedDatabase(app);

                var cache = app.Services.GetService<IDocumentCache>();

                using var scope = app.Services.CreateScope();
                var docSvc = scope.ServiceProvider.GetService<IDocumentService>();

                cache.Tree = docSvc.GetDocumentTree().Result;

                app.UseExceptionHandler("/Error");

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRouting();
                app.UseAuthorization();
                app.UseAuthentication();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });                

                app.Run();
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

        private static async void SeedDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var config = services.GetRequiredService<IConfiguration>();
            var docSettings = services.GetRequiredService<IOptions<DocumentOptions>>();

            if (docSettings.Value.Provider == DocumentOptions.Database)
            {
                var sqliteRepo = services.GetRequiredService<ISqliteRepository>();
                await sqliteRepo.EnsureCreated();
                DbInitializer.InitializeDatabase(sqliteRepo);                
            }
        }
    }
}
