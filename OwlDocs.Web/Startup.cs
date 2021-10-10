using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;

using Markdig;

using OwlDocs.Data;
using OwlDocs.Domain.Docs;
using OwlDocs.Models;
using OwlDocs.Web.Options;
using OwlDocs.Models.Options;
using OwlDocs.Web.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace OwlDocs.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            // add and validate options
            services.AddOptions<AuthOptions>()
                .Bind(Configuration.GetSection(AuthOptions.Authorization))
                .ValidateDataAnnotations();

            services.AddOptions<DocumentOptions>()
                .Bind(Configuration.GetSection(DocumentOptions.DocumentSettings))
                .ValidateDataAnnotations();

            // Add markdown services
            services.AddSingleton(md => new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());

            var docProvider = Configuration["DocumentSettings:Provider"];
            if (docProvider == "Database")
            {
                // add database services
                services.AddDbContext<OwlDocsContext>(options =>
                {
                    options.UseSqlServer(Configuration.GetConnectionString("TestConnection"));
                });

                services.AddScoped<IDocumentService, DbDocumentService>();
            }
            else if (docProvider == "File")
            {
                // add file services
                services.AddScoped<IDocumentService, FileDocumentService>();
            }

            services.AddSingleton<IDocumentCache, DocumentCache>();


            if (Configuration.GetValue<string>("Authoization:Type") == AuthorizationType.ActiveDirectory.ToString("D"))
            {
                services.AddAuthentication(IISDefaults.AuthenticationScheme);
            }

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthOptions.DocumentReaderPolicy,
                    policy => policy.Requirements.Add(new DocumentReadersRequirement()));

                options.AddPolicy(AuthOptions.DocumentWritersPolicy,
                    policy => policy.Requirements.Add(new DocumentWritersRequirement()));

                options.AddPolicy(AuthOptions.SiteAdminPolicy,
                    policy => policy.Requirements.Add(new SiteAdminRequirement()));

            });

            services.AddSingleton<IAuthorizationHandler, DocumentReadersHandler>();
            services.AddSingleton<IAuthorizationHandler, DocumentWritersHandler>();
            services.AddSingleton<IAuthorizationHandler, SiteAdminHandler>();

            // Add asp.net core required services
            services.AddMvc();

            services.AddDatabaseDeveloperPageExceptionFilter();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDocumentCache cache, IDocumentService docSvc)
        {
            cache.Tree = docSvc.GetDocumentTree().Result;

            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseHsts();
            }
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
        }
    }
}
