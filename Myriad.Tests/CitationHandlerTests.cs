using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Myriad.Parser;
using Myriad.Library;

namespace Myriad.Tests
{
    public struct Citations
    {
        public const string CommaCitation = "(Mt 24:14, 15)";
        public const string BrokenCommaCitation = "(Mt 24:14, 16)";
    }
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
            (List<Citation> citations, MarkedUpParagraph paragraph) = SetupCitation("(Mt 24:14)");
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
            (List<Citation> citations, MarkedUpParagraph paragraph) = SetupCitation("(Mt 24:14)");
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
            (List<Citation> citations, MarkedUpParagraph paragraph) = SetupCitation("(Mt 24)");
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

        [Test]
        public void TestCommaCitation()
        {
            (List<Citation> citations, MarkedUpParagraph paragraph) = 
                SetupCitation(Citations.CommaCitation);
            Assert.That(citations.Count > 0);
            if (citations.Count > 0)
            {
                var firstCitation = citations[Ordinals.first];
                var book = firstCitation.CitationRange.Book;
                int chapter = firstCitation.CitationRange.FirstChapter;
                int verse = firstCitation.CitationRange.FirstVerse;
                int lastverse = firstCitation.CitationRange.LastVerse;
                string label = paragraph.StringAt(firstCitation.Label);
                Assert.AreEqual(Citations.CommaCitation.Substring(1, Citations.CommaCitation.Length-2)
                    , label);
                Assert.That(firstCitation.CitationRange.Valid);
                Assert.That((book == 39 && chapter == 24 && verse == 14 && lastverse == 15),
                    () => {
                        return Bible.NamesTitleCase[book] + " " + chapter.ToString() + ":" + verse.ToString()
                    + "-" + lastverse.ToString();
                    });
            }
        }

        [Test]
        public void TestBrokenCommaCitation()
        {
            (List<Citation> citations, MarkedUpParagraph paragraph) =
                SetupCitation(Citations.BrokenCommaCitation);
            Assert.That(citations.Count > 0);
            if (citations.Count > 0)
            {
                var secondCitation = citations[Ordinals.second];
                int book = secondCitation.CitationRange.Book;
                int chapter = secondCitation.CitationRange.FirstChapter;
                int verse = secondCitation.CitationRange.FirstVerse;
                int lastverse = secondCitation.CitationRange.LastVerse;
                string label = paragraph.StringAt(secondCitation.Label);
                Assert.AreEqual("16", label);
                Assert.That(secondCitation.CitationRange.Valid);
                Assert.That((book == 39 && chapter == 24 && verse == 16 && lastverse == 16),
                    () => {
                        return Bible.NamesTitleCase[book] + " " + chapter.ToString() + ":" + verse.ToString()
                    + "-" + lastverse.ToString();
                    });
                var firstCitation = citations[Ordinals.first];
                 book = firstCitation.CitationRange.Book;
                 chapter = firstCitation.CitationRange.FirstChapter;
                 verse = firstCitation.CitationRange.FirstVerse;
                 lastverse = firstCitation.CitationRange.LastVerse;
                 label = paragraph.StringAt(firstCitation.Label);
                Assert.AreEqual("Mt 24:14", label);
                Assert.That(firstCitation.CitationRange.Valid);
                Assert.That((book == 39 && chapter == 24 && verse == 14 && lastverse == 14),
                    () => {
                        return Bible.NamesTitleCase[book] + " " + chapter.ToString() + ":" + verse.ToString()
                    + "-" + lastverse.ToString();
                    });
            }
        }

        private static (List<Citation> citations, MarkedUpParagraph paragraph) SetupCitation(string textOfCitation)
        {
            CitationHandler citationHandler = new CitationHandler();
            MarkedUpParagraph paragraph = new MarkedUpParagraph();
            paragraph.Text = textOfCitation;
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(1);
            mainRange.MoveEndTo(textOfCitation.Length-1);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
            return (citations, paragraph);
        }
    }
}
