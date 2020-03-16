using System;
using System.Collections.Generic;
using NUnit.Framework;
using Myriad;
using Myriad.Library;
using Myriad.Parser;
using Myriad.Data;

namespace Myriad.Tests
{
    [TestFixture]
    public class HomePageTests
    {
        //[Test]
        public void CanGetData()
        {
            IndexModel indexModel = new IndexModel();
            var paragraphs = indexModel.GetPageParagraphs();
            Assert.That(paragraphs.Count > Numbers.nothing);
        }
        [Test]
        public void ParserTests()
        {
            var parser = new MarkupParser<MarkedUpParagraph>();
            var paragraphs = new List<string>();
            paragraphs.Add("**bold**");
            var list = MarkedupParagraphList<MarkedUpParagraph>.CreateFrom(paragraphs);
            parser.Parse(list);
            Assert.That(parser.ParsedText.ToString() == "<b>bold</b>", () => { return parser.ParsedText.ToString(); });
        }
    }
}
