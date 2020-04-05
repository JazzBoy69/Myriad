using System;
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

        protected override string GetTitle()
        {
            throw new NotImplementedException();
        }

        protected override string PageScripts()
        {
            throw new NotImplementedException();
        }

        public async override void RenderBody(HTMLWriter writer)
        {
            throw new NotImplementedException();
            await AddPageTitleData();
        }

        public override void SetupNextPage()
        {
            throw new NotImplementedException();
        }

        public override void SetupPrecedingPage()
        {
            throw new NotImplementedException();
        }
    }
}
