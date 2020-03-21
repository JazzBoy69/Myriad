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
        private const string pageURL = "/Chapter";
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

        protected override Task RenderBody()
        {
            throw new NotImplementedException();
        }

        public override string GetURL()
        {
            return pageURL;
        }
    }
}
