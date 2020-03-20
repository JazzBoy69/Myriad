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
        public const string SimpleCitation = "(Mr 13:10)";
        public const string ChapterCitation = "(Mt 24)";
        public const string NumberedBookCitation = "(1Jo 5:3)";
        public const string RangeCitation = "(Mt 24:45-47)";
    }
    class CitationHandlerTests
    {
        [Test]
        public void SimpleCitation()
        {
            string citationText;
            citationText = Citations.SimpleCitation;
            TestSimpleCitation(citationText);
             citationText = Citations.NumberedBookCitation;
            TestSimpleCitation(citationText);
        }

        private static void TestSimpleCitation(string citationText)
        {
            (List<Citation> citations, MarkedUpParagraph paragraph) = SetupCitation(citationText);
            Assert.That(citations.Count > 0, () =>
            {
                return citationText;
            });
            if (citations.Count > 0)
            {
                int p = citationText.IndexOf(' ');
                int correctBook = Bible.IndexOfBook(citationText.Substring(Ordinals.second,
                    p - 1));
                int q = citationText.IndexOf(':');
                int correctChapter = Convert.ToInt32(citationText.Substring(p + 1, q - p - 1));
                int correctVerse = Convert.ToInt32(citationText.Substring(q + 1,
                    citationText.Length - q - 2));
                var firstCitation = citations[Ordinals.first];
                var book = firstCitation.CitationRange.Book;
                int chapter = firstCitation.CitationRange.FirstChapter;
                int verse = firstCitation.CitationRange.FirstVerse;
                Assert.That(book == correctBook && chapter == correctChapter && 
                    verse == correctVerse, () =>
                { return "book =" + book + "("+correctBook+") chapter=" + chapter + "("+
                    correctChapter+") verse=" + verse+"("+correctVerse+")"; });
                Assert.That(firstCitation.Label.Start == 1);
                Assert.That(firstCitation.Label.End == citationText.Length - 1, ()=>
                {
                    return firstCitation.Label.End.ToString()+"-"+citationText.Length;
                });
                string label = paragraph.StringAt(firstCitation.Label);
                Assert.AreEqual(citationText.Substring(Ordinals.second,
                  citationText.Length - 2), label);
            }
        }

        [Test]
        public void ChapterCitation()
        {
            (List<Citation> citations, MarkedUpParagraph paragraph) = SetupCitation(Citations.ChapterCitation);
            Assert.That(citations.Count > 0);
            if (citations.Count > 0)
            {
                var firstCitation = citations[Ordinals.first];
                var book = firstCitation.CitationRange.Book;
                int chapter = firstCitation.CitationRange.FirstChapter;
                int verse = firstCitation.CitationRange.FirstVerse;
                int lastverse = firstCitation.CitationRange.LastVerse;
                string label = paragraph.StringAt(firstCitation.Label);
                Assert.AreEqual(Citations.ChapterCitation.Substring(Ordinals.second,
                    Citations.ChapterCitation.Length-2), label);

                Assert.That((book == 39 && chapter == 24 && verse == 1 && lastverse == 51),
                    () =>
                    {
                        return Bible.AbbreviationsTitleCase[book] + " " + chapter.ToString() + ":" + verse.ToString()
                    + "-" + lastverse.ToString();
                    });
                Assert.That(firstCitation.CitationRange.Valid);
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
                Assert.AreEqual(Citations.CommaCitation.Substring(1, Citations.CommaCitation.Length - 2)
                    , label);
                Assert.That(firstCitation.CitationRange.Valid);
                Assert.That((book == 39 && chapter == 24 && verse == 14 && lastverse == 15),
                    () =>
                    {
                        return Bible.NamesTitleCase[book] + " " + chapter.ToString() + ":" + verse.ToString()
                    + "-" + lastverse.ToString();
                    });
            }
        }

        [Test]
        public void TestRangeCitation()
        {
            (List<Citation> citations, MarkedUpParagraph paragraph) =
                SetupCitation(Citations.RangeCitation);
            Assert.That(citations.Count > 0);
            if (citations.Count > 0)
            {
                var firstCitation = citations[Ordinals.first];
                int book = firstCitation.CitationRange.Book;
                int chapter = firstCitation.CitationRange.FirstChapter;
                int verse = firstCitation.CitationRange.FirstVerse;
                int lastverse = firstCitation.CitationRange.LastVerse;
                string label = paragraph.StringAt(firstCitation.Label);
                Assert.That(firstCitation.CitationRange.Valid);
                Assert.That((book == 39 && chapter == 24 && verse == 45 && lastverse == 47),
                    () =>
                    {
                        return Bible.NamesTitleCase[book] + " " + chapter.ToString() + ":" + verse.ToString()
                    + "-" + lastverse.ToString();
                    });
                Assert.AreEqual("Mt 24:45-47", label);
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
                    () =>
                    {
                        return Bible.NamesTitleCase[book] + " " + chapter.ToString() + ":" + verse.ToString()
                    + "-" + lastverse.ToString();
                    });
                var firstCitation = citations[Ordinals.first];
                book = firstCitation.CitationRange.Book;
                chapter = firstCitation.CitationRange.FirstChapter;
                verse = firstCitation.CitationRange.FirstVerse;
                lastverse = firstCitation.CitationRange.LastVerse;
                label = paragraph.StringAt(firstCitation.Label);
                Assert.That(firstCitation.CitationRange.Valid);
                Assert.That((book == 39 && chapter == 24 && verse == 14 && lastverse == 14),
                    () =>
                    {
                        return Bible.NamesTitleCase[book] + " " + chapter.ToString() + ":" + verse.ToString()
                    + "-" + lastverse.ToString();
                    });
                Assert.AreEqual("Mt 24:14", label);
            }
        }

        private static (List<Citation> citations, MarkedUpParagraph paragraph) SetupCitation(string textOfCitation)
        {

            CitationHandler citationHandler = new CitationHandler();
            MarkedUpParagraph paragraph = new MarkedUpParagraph();
            paragraph.Text = textOfCitation;
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(1);
            mainRange.MoveEndTo(textOfCitation.Length - 1);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
            return (citations, paragraph);

        }
    }
}
