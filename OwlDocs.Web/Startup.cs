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
            services.AddDbContext<OwlDocsContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("TestConnection"));
            });

            var docProvider = Configuration["DocumentProvider"];
            if (docProvider == "Database")
            {
                services.AddScoped<IDocumentService, FileDocumentService>();
            }
            else if (docProvider == "File")
            {
                var root = Configuration["DocumentProviderSettings:DirectoryRoot"];
                services.AddScoped<IDocumentService>(s => new FileDocumentService(root));
            }

            services.AddSingleton(md => new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());

            services.AddMvc();

            services.AddDatabaseDeveloperPageExceptionFilter();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

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
