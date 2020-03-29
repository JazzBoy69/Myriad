﻿using System;
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
        List<string> paragraphs;
        MarkupParser parser;
        private void RunTest()
        {
            string citationText;
            List<Citation> citation = CitationConverter.FromString("3Jo 1, 5-8");
            citationText = CitationConverter.ToString(citation);
            Console.WriteLine(citationText);
        }

        private void InitializeParser()
        {
            parser = new MarkupParser(new HTMLStringBuilder());
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            paragraphs = new List<string>();
        }
    }
}
