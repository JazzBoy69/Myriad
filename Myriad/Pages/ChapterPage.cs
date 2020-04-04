using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Parser;

namespace Myriad.Pages
{
    public class ChapterPage : ScripturePage
    {
        public const string pageURL = "/Chapter";
        internal ChapterPage()
        {
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

        public override string GetURL()
        {
            return pageURL;
        }

        protected override CitationTypes GetCitationType()
        {
            throw new NotImplementedException();
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
