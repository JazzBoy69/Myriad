using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Feliciana.ResponseWriter;
using Myriad.Parser;
using Myriad.Pages;
using Myriad.Library;

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
        public async Task RenderVerse()
        {
            VersePage page = new VersePage();
            page.SetResponse(DefaultResponse());
            page.SetCitation(new Citation(654639872, 654640127));
            await page.RenderPage();
        }

        [Test]
        public async Task ParserFormatting()
        {
            InitializeParser();
            paragraphs.Add("testing **bold** //italic// **//bold italic//**");
            await parser.ParseParagraph(paragraphs[0], 0);
            string result = parser.ParsedText.ToString();
            Assert.AreEqual("testing <b>bold</b> <i>italic</i> <b><i>bold italic</i></b>", result);
        }
        private void InitializeParser()
        {
            parser = new MarkupParser(Writer.New());
            paragraphs = new List<string>();
        }

    }
}
