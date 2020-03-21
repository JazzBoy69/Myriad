﻿using Microsoft.AspNetCore.Http;
using Myriad.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Pages
{
    public class TextPage : ScripturePage
    {
        const string pageURL = "/Text";

        public override string GetURL()
        {
            return pageURL;
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
    }
}
