using System;
using System.Threading.Tasks;
using Feliciana.ResponseWriter;
using Myriad.Library;


namespace Myriad.Pages
{
    public class VersePage : ScripturePage
    {
        public const string pageURL = "/Verse";
        public override string GetURL()
        {
            return pageURL;
        }

        protected override CitationTypes GetCitationType()
        {
            throw new NotImplementedException();
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

        public override void SetupNextPage()
        {
            throw new NotImplementedException();
        }

        public override void SetupPrecedingPage()
        {
            throw new NotImplementedException();
        }

        public override Task AddTOC(HTMLWriter writer)
        {
            throw new NotImplementedException();
        }

        public override void LoadTOCInfo()
        {
            throw new NotImplementedException();
        }
    }
}
