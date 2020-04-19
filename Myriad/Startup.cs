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
using Feliciana.Library;
using Feliciana.ResponseWriter;
using Myriad.Library;
using Myriad.Pages;
using Myriad.Parser;

namespace Myriad
{
    //todo Change all object methods to static. Use separate info object to pass state between functions
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSession();
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
        }

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
                //todo write change log
                if (context.Request.Path == "/SynonymSearch")
                {
                    var searchPage = new SearchPage();
                    await searchPage.LoadQueryInfo(context.Request.Query);
                    if (!searchPage.IsValid()) return;
                    searchPage.SetResponse(context.Response);
                    await searchPage.WriteSynonymResults(Writer.New(context.Response));
                    return;
                }
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
                    await EditParagraph.GetPlainText(context);
                    return;
                }
                if (path == EditParagraph.setDataURL)
                {
                    await EditParagraph.SetText(context);
                    return;
                }
                CommonPage page = await RequestedPage(context);
                await page.RenderPage();
            });

        }

        public async Task HandleTOCRequest(HttpContext context)
        {
            CommonPage partialPage = await RequestedPage(context);
            await partialPage.LoadTOCInfo(context);
            await partialPage.AddTOC(Writer.New(context.Response));
        }

        private async Task HandlePartialRequest(HttpContext context)
        {
            CommonPage partialPage = await RequestedPage(context);
            if (context.Request.Query.ContainsKey("next"))
            {
                PaginationPage paginationPage = (PaginationPage)partialPage;
                await paginationPage.SetupNextPage();
                await paginationPage.RenderBody(Writer.New(context.Response));
                return;
            }
            if (context.Request.Query.ContainsKey("preceding"))
            {
                PaginationPage paginationPage = (PaginationPage)partialPage;
                await paginationPage.SetupPrecedingPage();
                await paginationPage.RenderBody(Writer.New(context.Response));
                return;
            }
            await partialPage.RenderBody(Writer.New(context.Response));
        }

        private async Task<CommonPage> RequestedPage(HttpContext context)
        {
            string path = context.Request.Path;
            var query = context.Request.Query;

            if ((path == SearchPage.pageURL) && (query.ContainsKey("q")))
            {
                Citation citation = CitationConverter.FromString(query["q"])[Ordinals.first];
                if (citation.CitationType == CitationTypes.Invalid)
                {
                    CommonPage searchPage = new SearchPage();
                    await searchPage.LoadQueryInfo(query);
                    if (!searchPage.IsValid()) searchPage = new IndexPage();
                    searchPage.SetResponse(context.Response);
                    return searchPage;
                }

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
            await page.LoadQueryInfo(query);
            if (!page.IsValid()) page = new IndexPage();
            page.SetResponse(context.Response);
            return page;
        }
    }
}
