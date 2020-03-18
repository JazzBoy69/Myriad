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
            MarkedUpParagraph  paragraph = new MarkedUpParagraph();
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
        public void TestCitationSetup()
        {
            (List<Citation> citations, MarkedUpParagraph paragraph) = SetupSimpleCitation();
            Assert.That((citations.Count > 0), () => { return "no citations returned"; });
            if (citations.Count > 0)
            {
                var firstCitation = citations[Ordinals.first];
                Assert.That(firstCitation.CitationType != CitationTypes.Invalid,
                    () => { return "Invalid citation"; });
            }
        }

        [Test]
        public void SimpleCitation()
        {
            (List<Citation> citations, MarkedUpParagraph paragraph) = SetupSimpleCitation();
            if (citations.Count > 0)
            {
                var firstCitation = citations[Ordinals.first];
                var book = firstCitation.CitationRange.Book;
                int chapter = firstCitation.CitationRange.FirstChapter;
                int verse = firstCitation.CitationRange.FirstVerse;
                Assert.That(book == 39 && chapter == 24 && verse == 14, () =>
                { return "book =" + book + " chapter=" + chapter + " verse=" + verse; });
                string label = paragraph.StringAt(firstCitation.Label);
                Assert.AreEqual("Mt 24:14", label);
            }

        }

        [Test]
        public void ChapterCitation()
        {
            (List<Citation> citations, MarkedUpParagraph paragraph) = SetupChapterCitation();
            Assert.That(citations.Count > 0);
            if (citations.Count > 0)
            {
                var firstCitation = citations[Ordinals.first];
                var book = firstCitation.CitationRange.Book;
                int chapter = firstCitation.CitationRange.FirstChapter;
                int verse = firstCitation.CitationRange.FirstVerse;
                int lastverse = firstCitation.CitationRange.LastVerse;
                string label = paragraph.StringAt(firstCitation.Label);
                Assert.That("Mt 24" == label, () => { return firstCitation.Label.Start+"-"
                    +firstCitation.Label.End; });
                Assert.That(firstCitation.CitationRange.Valid);
                Assert.That((book == 39 && chapter == 24 && verse == 1 && lastverse == 51),
                    () => { return Bible.NamesTitleCase[book] + " " + chapter.ToString() + ":" + verse.ToString()
                        + "-" + lastverse.ToString(); });
            }
        }

        private static (List<Citation> citations, MarkedUpParagraph paragraph) SetupSimpleCitation()
        {
            CitationHandler citationHandler = new CitationHandler();
            MarkedUpParagraph paragraph = new MarkedUpParagraph();
            paragraph.Text = "(Mt 24:14)";
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(1);
            mainRange.MoveEndTo(9);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
            return (citations, paragraph);
        }

        private static (List<Citation> citations, MarkedUpParagraph paragraph) SetupChapterCitation()
        {
            CitationHandler citationHandler = new CitationHandler();
            MarkedUpParagraph paragraph = new MarkedUpParagraph();
            paragraph.Text = "(Mt 24)";
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(1);
            mainRange.MoveEndTo(6);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
            return (citations, paragraph);
        }
    }
}
