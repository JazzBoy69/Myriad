using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Feliciana.ResponseWriter;
using Myriad.Library;


namespace Myriad.Pages
{
    public class VersePage : ScripturePage
    {
        public const string pageURL = "/Verse";

        //todo implement verse page
        public override string GetURL()
        {
            return pageURL;
        }

        protected override CitationTypes GetCitationType()
        {
            return CitationTypes.Verse;
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

        public override Task SetupNextPage()
        {
            throw new NotImplementedException();
        }

        public override Task SetupPrecedingPage()
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
