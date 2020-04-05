﻿using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Myriad.Parser;
using Myriad.Pages;
using Myriad.Writer;

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
            IndexPage page = new IndexPage();

            var writer = WriterReference.New();
            page.RenderBody(writer);
        }

        private void InitializeParser()
        {
            parser = new MarkupParser(WriterReference.New());
            paragraphs = new List<string>();
        }
    }
}
