using System;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Pages
{
    public class ScriptureErrorPage : ScripturePage
    {
        public override string GetURL()
        {
            throw new NotImplementedException();
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

        public override Task RenderBody(HTMLWriter writer)
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
