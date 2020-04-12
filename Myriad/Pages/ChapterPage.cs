using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Feliciana.ResponseWriter;
using Myriad.Library;

namespace Myriad.Pages
{
    public class ChapterPage : ScripturePage
    {
        public const string pageURL = "/Chapter";

        //todo implement chapter page
        internal ChapterPage()
        {
        }

        protected override async Task WriteTitle(HTMLWriter writer)
        {
            throw new NotImplementedException();
        }

        protected override string PageScripts()
        {
            throw new NotImplementedException();
        }

        public async override Task RenderBody(HTMLWriter writer)
        {
            throw new NotImplementedException();
            await AddPageTitleData(writer);
        }

        public override string GetURL()
        {
            return pageURL;
        }

        protected override CitationTypes GetCitationType()
        {
            throw new NotImplementedException();
        }

        public override async Task SetupNextPage()
        {
            throw new NotImplementedException();
        }

        public override async Task SetupPrecedingPage()
        {
            throw new NotImplementedException();
        }

        public override Task AddTOC(HTMLWriter writer)
        {
            throw new NotImplementedException();
        }

        public override Task LoadTOCInfo(HttpContext context)
        {
            throw new NotImplementedException();
        }

    }
}
