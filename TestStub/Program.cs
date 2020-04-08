using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Myriad.Parser;
using Myriad.Pages;
using Myriad.Library;

namespace TestStub
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.RunTest();
        }
        private HttpResponse DefaultResponse()
        {
            return new DefaultHttpContext().Response;
        }
        async private void RunTest()
        {
            TextPage page = new TextPage();
            page.SetResponse(DefaultResponse());
            page.SetCitation(new Citation(319422720, 319422739));
            await page.RenderPage();
        }

    }
}
