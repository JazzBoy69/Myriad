using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Myriad.Parser;
using Myriad.Data;
using Myriad;

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
        private void RunTest()
        {
            List<string> paragraphs = ReaderProvider.Reader()
                .GetData<string>(DataOperation.ReadNavigationPage, "home");
            var response = DefaultResponse();
            var parser = new NavigationParser(new HTMLStringBuilder());
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            parser.Parse(paragraphs);
            string result = parser.ParsedText;
        }
    }
}
