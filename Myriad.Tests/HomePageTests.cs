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
        [Test]
        public void CanGetData()
        {
            IndexModel indexModel = new IndexModel();
            var paragraphs = indexModel.GetPageParagraphs();
            Assert.That(paragraphs.Count > Numbers.nothing);
        }
        [Test]
        public void CanParseParagraph()
        {
            var paragraphs = new List<MarkedUpParagraph>();
            MarkedUpParagraph paragraph = new MarkedUpParagraph("A Bible verse (for example “{Mt 5:1}”). A Bible chapter (for example “{Mt 13}”). A verse followed by an exclamation mark (for example “{Mr 2:1!|Mr 2:1!}”).");
            paragraphs.Add(paragraph);
            paragraph = new MarkedUpParagraph("==Searching for Information==");
            paragraphs.Add(paragraph);
            IndexModel indexModel = new IndexModel();
            indexModel.Parse(paragraphs);
            Assert.That(indexModel.PageBody.ToString() == "<section><p>A Bible verse (for example “<a class=link HREF=/Text?start=654639360&end=654639380>Mt 5:1</a>”). A Bible chapter (for example “<a HREF=/Chapter?start=655163648&end=655178255>Mt 13</a>”). A verse followed by an exclamation mark (for example “<a HREF=/Verse?verse=671219968>Mr 2:1!</a>”).</p></section><div class='clear'></div><section><h3>Searching for Information</h3></section><div class='clear'></div>");
        }
    }
}
