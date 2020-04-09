using System;
using System.Threading.Tasks;
using Feliciana.ResponseWriter;
using Myriad.Library;

namespace Myriad.Pages
{
    public class ChapterPage : ScripturePage
    {
        public const string pageURL = "/Chapter";
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

        public override Task LoadTOCInfo()
        {
            throw new NotImplementedException();
        }

    }
}
