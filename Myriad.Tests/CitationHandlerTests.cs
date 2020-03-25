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
        public const string CommaCitation = "Mt 24:14, 15";
        public const string BrokenCommaCitation = "Mt 24:14, 16";
        public const string SimpleCitation = "Mr 13:10";
        public const string ChapterCitation = "Mt 24";
        public const string NumberedBookCitation = "1 John 5:3";
        public const string RangeCitation = "Mt 24:45-47";
        public const string BangCitation = "Mr 2:1!";
    }
    class CitationHandlerTests
    {
        [Test]
        public void SimpleCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.SimpleCitation);
            citationText = CitationConverter.ToString(citation);
            TestCitation(citationText, citation);
        }

        [Test]
        public void CommaCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.CommaCitation);
            citationText = CitationConverter.ToString(citation);
            TestCitation(citationText, citation);
        }

        [Test]
        public void BrokenCommaCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.BrokenCommaCitation);
            citationText = CitationConverter.ToString(citation);
            TestCitation(citationText, citation);
        }

        [Test]
        public void ChapterCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.ChapterCitation);
            citationText = CitationConverter.ToString(citation);
            TestCitation(citationText, citation);
        }

        [Test]
        public void NumberedBookCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.NumberedBookCitation);
            citationText = CitationConverter.ToString(citation);
            TestCitation(citationText, citation);
        }

        [Test]
        public void RangeCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.RangeCitation);
            citationText = CitationConverter.ToString(citation);
            TestCitation(citationText, citation);
        }

        [Test]
        public void BangCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.BangCitation);
            citationText = CitationConverter.ToString(citation);
            TestCitation(citationText, citation);
        }
        private static void TestCitation(string citationText, Citation citation)
        {
            (List<Citation> citations, MarkedUpParagraph paragraph) = SetupCitation(citationText);
            Assert.That(citations.Count > 0, () =>
            {
                return citationText;
            });
            if (citations.Count > 0)
            {
                int correctBook = citation.CitationRange.Book;
                int correctChapter = citation.CitationRange.FirstChapter;
                int correctVerse = citation.CitationRange.FirstVerse;
                var firstCitation = citations[Ordinals.first];
                var book = firstCitation.CitationRange.Book;
                int chapter = firstCitation.CitationRange.FirstChapter;
                int verse = firstCitation.CitationRange.FirstVerse;
                Assert.That(book == correctBook && chapter == correctChapter && 
                    verse == correctVerse, () =>
                { return "book =" + book + "("+correctBook+") chapter=" + chapter + "("+
                    correctChapter+") verse=" + verse+"("+correctVerse+")"; });
                Assert.That(firstCitation.Label.Start == 0);
                Assert.That(firstCitation.Label.End == citationText.Length-1, ()=>
                {
                    return firstCitation.Label.End.ToString()+"-"+(citationText.Length-1);
                });
                string label = paragraph.StringAt(firstCitation.Label);
                Assert.AreEqual(citationText, label);
                 correctChapter = citation.CitationRange.LastChapter;
                 correctVerse = citation.CitationRange.LastVerse;
                 chapter = firstCitation.CitationRange.LastChapter;
                 verse = firstCitation.CitationRange.LastVerse;
                Assert.That(book == correctBook && chapter == correctChapter &&
                    verse == correctVerse, () =>
                    {
                        return "book =" + book + "(" + correctBook + ") chapter=" + chapter + "(" +
                          correctChapter + ") verse=" + verse + "(" + correctVerse + ")";
                    });
            }
        }

        private static (List<Citation> citations, MarkedUpParagraph paragraph) SetupCitation(string textOfCitation)
        {

            CitationHandler citationHandler = new CitationHandler();
            MarkedUpParagraph paragraph = new MarkedUpParagraph();
            paragraph.Text = textOfCitation;
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(0);
            mainRange.MoveEndTo(textOfCitation.Length - 1);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
            return (citations, paragraph);

        }


    }
}
