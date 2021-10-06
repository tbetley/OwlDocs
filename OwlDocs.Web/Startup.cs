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

using Microsoft.EntityFrameworkCore;
using Markdig;

using OwlDocs.Data;
using OwlDocs.Domain.Docs;
using OwlDocs.Models;

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
            // Add markdown services
            services.AddSingleton(md => new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());

            var docProvider = Configuration["DocumentProvider"];
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
