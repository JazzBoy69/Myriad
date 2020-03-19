﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Text;
using Myriad.Parser;

namespace Myriad.Tests
{
    class ParserTestscs
    {
        List<string> paragraphs;
        MarkupParser parser;
        private HttpResponse DefaultResponse()
        {
            return new DefaultHttpContext().Response;
        }

        [Test]
        async public Task ParserFormatting()
        {
            InitializeParser();
            paragraphs.Add("testing **bold** //italic// **//bold italic//**");
            await parser.Parse(paragraphs);
            string result = parser.ParsedText.ToString();
            Assert.AreEqual("<section><p>testing <b>bold</b> <i>italic</i> <b><i>bold italic</i></b></p></section>", result);
        }
        private void InitializeParser()
        {
            parser = new MarkupParser(new HTMLStringBuilder());
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            paragraphs = new List<string>();
        }

    }
}