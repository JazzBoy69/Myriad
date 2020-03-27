using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using Myriad.Pages;
using Myriad.Parser;
using Myriad.Library;

namespace Myriad.Tests
{
    class TextPageTests
    {
        [Test]
        public void CanReadData()
        {
            TextPage textPage = new TextPage();
            textPage.SetResponse(new DefaultHttpContext().Response);
            var paragraphs = textPage.ReadParagraphs(1);
            Assert.That(paragraphs.Count > 0);
            var links = textPage.ReadLinks(1);
            Citation citation = new Citation(links[Ordinals.first].start, links[Ordinals.first].end);
            Assert.That(links.Count > 0);
            var keywords = textPage.ReadKeywords(citation);
            Assert.That(keywords.Count > 0);
        }
    }
}
