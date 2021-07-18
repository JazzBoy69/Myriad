using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Myriad.Parser;
using Myriad.Pages;
using Myriad.Library;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Myriad.CitationHandlers;
using Myriad.Search;

namespace TestStub
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.RunTest();
        }
        private HttpResponse DefaultResponse()
        {
            return new DefaultHttpContext().Response;
        }
        async private void RunTest()
        {
                Chrono page = new Chrono();
                page.SetResponse(DefaultResponse());
                await page.SetCitation(new Citation(309002496, 309002508));
                await page.RenderPage();
        }

        async private void RunTest2()
        {
            var searchPage = new SearchPage();
            var pageInfo = new SearchPageInfo();
            //pageInfo.SetSearchQuery("?q=Jehovah+is+the+true+God");
            pageInfo.SetSearchQuery("Jehovah");
            (CitationRange r, string q) = SearchPage.SearchRange(pageInfo.SearchQuery);
            pageInfo.SetCitationRange(r);
            //pageInfo.SetQuery("Jehovah is the true God");
            pageInfo.SetQuery("Jehovah");
            searchPage.SetPageInfo(pageInfo);
            searchPage.SetResponse(DefaultResponse());
            if (!searchPage.IsValid()) return;
            await searchPage.RenderBody(Writer.New());
            pageInfo.SetQuery("");
        }

    }
}
