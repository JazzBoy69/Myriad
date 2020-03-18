using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using Myriad.Parser;

namespace Myriad
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSession();
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env == null) return;
            if (env.EnvironmentName == Environments.Development)
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSession();
            app.UseStaticFiles();
            app.Run(async context => 
            {
                await WriteHeader(context.Response);
                RenderPage(context.Response);
                await context.Response.WriteAsync(LayoutHTML.close);
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }

        async public void TestPage(HttpResponse response)
        {
            response.Clear();
            await WriteHeader(response);
            RenderPage(response);
            await response.WriteAsync(LayoutHTML.close);
        }

        private static async Task WriteHeader(HttpResponse response)
        {
            await response.WriteAsync(LayoutHTML.startOfPage);
            await response.WriteAsync("<title>Myriad - Index</title>");
            await response.WriteAsync(LayoutHTML.header);
        }

        public void RenderPage(HttpResponse response)
        {
            var paragraphs = GetPageParagraphs();
            var parser = new MarkupParser(new HTMLResponseWriter(response));
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            parser.Parse(paragraphs);
        }
        public List<string> GetPageParagraphs()
        {
            return TestParagraphs();
        }

        private List<string> TestParagraphs()
        {
            var results = new List<string>();
            results.Add("==Heading==");
            results.Add("test paragraph **bold** //italic//");
            return results;
        }
    }
}
