using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Parser;

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

        protected override string GetTitle()
        {
            throw new NotImplementedException();
        }

        protected override string PageScripts()
        {
            throw new NotImplementedException();
        }

        public override void RenderBody(HTMLWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
