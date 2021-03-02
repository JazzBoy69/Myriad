using System;
using System.IO;
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
using Feliciana.HTML;
using Myriad.Library;
using Myriad.Pages;
using Myriad.Parser;
using System.Drawing;

namespace Myriad
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSession();
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
            ImageElement.SetStaticInformation(System.IO.Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot", "pictures"), "pictures", JavaScriptFunctions.OpenModalPicture);

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
                string path = context.Request.Path;
                if (path == Sidebar.pageURL)
                {
                    await Sidebar.HandleRequest(Writer.New(context.Response), context.Request.Query);
                    return;
                }
                if (path == ArticlePage.addArticleURL)
                {
                    ArticlePage articlePage = new ArticlePage();
                    await articlePage.AddArticle(Writer.New(context.Response), context.Request.Query);
                    return;
                }
                if (path.Contains("/Edit/"))
                {
                    await HandleEditRequest(context, path.Replace("/Edit", ""));
                    return;
                }
                if (path == "/SynonymSearch")
                {
                    var searchPage = new SearchPage();
                    await searchPage.LoadQueryInfo(context.Request.Query);
                    if (!searchPage.IsValid()) return;
                    searchPage.SetResponse(context.Response);
                    await searchPage.WriteSynonymResults(Writer.New(context.Response));
                    return;
                }
                if (context.Request.Query.ContainsKey("originalword"))
                {
                    var versePage = new VersePage();
                    await versePage.LoadQueryInfo(context.Request.Query);
                    if (context.Request.Query.ContainsKey("accept"))
                    {
                        context.Request.Form.TryGetValue("text", out var text);
                        await versePage.UpdateOriginalWordComments(Writer.New(context.Response), text);
                        return;
                    }
                    await versePage.WriteOriginalWordComments(Writer.New(context.Response));
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

        private async Task HandleEditRequest(HttpContext context, string path)
        {
            CommonPage page = CreatePageFromPath(path);
            if (context.Request.Query.ContainsKey("accept"))
            {
                await page.HandleAcceptedEdit(context);
                return;
            }
            await page.HandleEditRequest(context);
        }


        public async Task HandleTOCRequest(HttpContext context)
        {
            CommonPage partialPage = await RequestedPage(context);
            await partialPage.LoadTOCInfo(context);
            await partialPage.WriteTOC(Writer.New(context.Response));
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
            if (context.Request.Query.ContainsKey("up"))
            {
                PaginationPage paginationPage = (PaginationPage)partialPage;
                await paginationPage.SetupParentPage();
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
                string searchQuery = query["q"];
                Citation citation = CitationConverter.FromString(searchQuery)[Ordinals.first];
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

                if (citation.CitationType == CitationTypes.Chapter)
                {
                    query = new QueryCollection(new Dictionary<string, StringValues>()
                    {
                        { "start", citation.CitationRange.StartID.ToString() },
                        {"end", citation.CitationRange.EndID.ToString() },
                        {"navigating", "true" }
                    });
                }
                else
                {
                    query = new QueryCollection(new Dictionary<string, StringValues>()
                    {
                        { "start", citation.CitationRange.StartID.ToString() },
                        {"end", citation.CitationRange.EndID.ToString() }
                    });
                }
            }

            CommonPage page = CreatePageFromPath(path);
            await page.LoadQueryInfo(query);
            if (!page.IsValid()) page = new IndexPage();
            page.SetResponse(context.Response);
            return page;
        }

        private static CommonPage CreatePageFromPath(string path)
        {
            CommonPage page;
            switch (path)
            {
                case ArticlePage.pageURL:
                    {
                        page = new ArticlePage();
                        break;
                    }
                case Chrono.pageURL:
                    {
                        page = new Chrono();
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

            return page;
        }
    }
}
