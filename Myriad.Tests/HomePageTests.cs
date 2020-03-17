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
            var parser = new MarkupParser();
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            var paragraphs = new List<string>();
            paragraphs.Add("testing **bold**");
            parser.Parse(paragraphs);
            string result = parser.ParsedText.ToString();
            Console.WriteLine(result);
            Assert.AreEqual("testing <b>bold</b>", result);
        }
    }
}
