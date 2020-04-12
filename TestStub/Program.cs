using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Myriad.Parser;
using Myriad.Library;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Myriad.CitationHandlers;

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
            string citationText;
            var citations = CitationConverter.FromString("Mt 24:14.preached");
            citationText = await CitationConverter.ToString(citations);
            Console.WriteLine(citationText);
        }

    }
}
