using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Myriad.Parser;
using Myriad.Library;

namespace Myriad.Tests
{
    class CitationHandlerTests
    {
        [Test]
        public void CitationBook()
        {
            CitationHandler citationHandler = new CitationHandler();
            MarkedUpParagraph paragraph = new MarkedUpParagraph();
            paragraph.Text = "(Mt 24:14)";
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(1);
            mainRange.MoveEndTo(9);
            var bookRange = citationHandler.GetBookRange(mainRange, paragraph);
            Assert.AreEqual(paragraph.StringAt(bookRange), "Mt");
            int book = CitationHandler.GetBookIndex(bookRange, paragraph);
            Assert.That(book == 39);
            Citation citation = new Citation(book, 1, 1);
            Assert.That(citation.CitationType == CitationTypes.Text);
        }
        [Test]
        public void CitationSetup()
        {
            List<Citation> citations = SetupSimpleCitation();
            Assert.That((citations.Count > 0), () => { return "no citations returned"; });
            if (citations.Count > 0)
            {
                var firstCitation = citations[Ordinals.first];
                Assert.That(firstCitation.CitationType != CitationTypes.Invalid,
                    () => { return "Invalid citation"; });
            }
        }

        private static List<Citation> SetupSimpleCitation()
        {
            CitationHandler citationHandler = new CitationHandler();
            MarkedUpParagraph paragraph = new MarkedUpParagraph();
            paragraph.Text = "(Mt 24:14)";
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(1);
            mainRange.MoveEndTo(9);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
            return citations;
        }

        [Test]
        public void SimpleCitation()
        {
            List<Citation> citations = SetupSimpleCitation();
            if (citations.Count > 0)
            {
                var firstCitation = citations[Ordinals.first];
                var book = firstCitation.CitationRange.Book;
                int chapter = firstCitation.CitationRange.FirstChapter;
                int verse = firstCitation.CitationRange.FirstVerse;
                Assert.That(book == 39 && chapter == 24 && verse == 14, () =>
                { return "book =" + book + " chapter=" + chapter + " verse=" + verse; });
            }
        }
    }
}
