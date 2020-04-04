using Microsoft.AspNetCore.Http;
using Myriad.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public override void RenderNextPage()
        {
            throw new NotImplementedException();
        }

        public override void RenderPrecedingPage()
        {
            throw new NotImplementedException();
        }
    }
}
