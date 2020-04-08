using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Hosting;
using FelicianaLibrary;
using Myriad.Library;
using Myriad.Pages;
using Myriad.Parser;
using Myriad.Writer;

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
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    const int durationInSeconds = 60 * 60 * 24*360;
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=" + durationInSeconds;
                }
            });
            app.Run(async context =>
            {
                if (context.Request.Query.ContainsKey("toc"))
                {
                    await HandleTOCRequest(context);
                    return;
                }
                if (context.Request.Query.ContainsKey("partial"))
                {
                    await HandlePartialRequest(context);
                    return;
                }
                string path = context.Request.Path;
                if (path == EditParagraph.getDataURL)
                {
                    EditParagraph.GetPlainText(context);
                    return;
                }
                if (path == EditParagraph.setDataURL)
                {
                    EditParagraph.SetText(context);
                    return;
                }
                CommonPage page = RequestedPage(context);
                await page.RenderPage();
            });

        }

        public async Task HandleTOCRequest(HttpContext context)
        {
            CommonPage partialPage = RequestedPage(context);
            await partialPage.LoadTOCInfo();
            await partialPage.AddTOC(WriterReference.New(context.Response));
        }

        private async Task HandlePartialRequest(HttpContext context)
        {
            CommonPage partialPage = RequestedPage(context);
            if (context.Request.Query.ContainsKey("next"))
            {
                ScripturePage scripturePage = (ScripturePage)partialPage;
                scripturePage.SetupNextPage();
                await scripturePage.RenderBody(WriterReference.New(context.Response));
                return;
            }
            if (context.Request.Query.ContainsKey("preceding"))
            {
                ScripturePage scripturePage = (ScripturePage)partialPage;
                scripturePage.SetupPrecedingPage();
                await scripturePage.RenderBody(WriterReference.New(context.Response));
                return;
            }
            await partialPage.RenderBody(WriterReference.New(context.Response));
            return;
        }

        private CommonPage RequestedPage(HttpContext context)
        {
            string path = context.Request.Path;
            var query = context.Request.Query;

            if ((path == SearchPage.pageURL) && (query.ContainsKey("q")))
            {
                Citation citation = CitationConverter.FromString(query["q"])[Ordinals.first]; ;
                if (citation.CitationType != CitationTypes.Invalid)
                {
                    switch (citation.CitationType)
                    {
                        case CitationTypes.Chapter:
                            {
                                path = ChapterPage.pageURL;
                                break;
                            }
                        case CitationTypes.Text:
                            {
                                path = TextPage.pageURL;
                                break;
                            }
                        case CitationTypes.Verse:
                            {
                                path = VersePage.pageURL;
                                break;
                            }
                    }
                    query = new QueryCollection(new Dictionary<string, StringValues>()
                    {
                        { "start", citation.CitationRange.StartID.ToString() },
                        {"end", citation.CitationRange.EndID.ToString() }
                    });
                }
            }

            CommonPage page;
            switch (path)
            {
                case ArticlePage.pageURL:
                    {
                        page = new ArticlePage();
                        break;
                    }
                case ChapterPage.pageURL:
                    {
                        page = new ChapterPage();
                        break;
                    }
                case TextPage.pageURL:
                    {
                        page = new TextPage();
                        break;
                    }
                case VersePage.pageURL:
                    {
                        page = new VersePage();
                        break;
                    }
                default:
                    page = new IndexPage();
                    break;
            }
            page.LoadQueryInfo(query);
            if (!page.IsValid()) page = new IndexPage();
            page.SetResponse(context.Response);
            return page;
        }
    }
}
