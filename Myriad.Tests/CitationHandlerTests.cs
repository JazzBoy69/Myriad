using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using NUnit.Framework;
using FelicianaLibrary;
using FelicianaHTML;
using ResponseWriter;
using Myriad.Parser;
using Myriad.Library;
using Myriad.CitationHandlers;

namespace Myriad.Tests
{
    public struct Citations
    {
        public const string CommaCitation = "Mt 24:14, 15";
        public const string BrokenCommaCitation = "Mt 24:14, 16-18";
        public const string BrokenCommaCitationResult = "Mt&nbsp;24:14,&nbsp;16-18";
        public const string SimpleCitation = "Mr 13:10";
        public const string ChapterCitation = "Mt 24";
        public const string NumberedBookCitation = "1 John 5:3";
        public const string NumberedBookCitationResult = "1Jo&nbsp;5:3";
        public const string RangeCitation = "Mt 24:45-47";
        public const string BangCitation = "Mr 2:1!";
        public const string BangCitationResult = "Mr&nbsp;2:1";
        public const string ShortBookCitation = "2Jo 10";
        public const string ShortBookCommaCitation = "2Jo 10, 11";
        public const string ShortBookBrokenCitation = "3Jo 1, 5-8";
        public const string MultipleCitations = "Mt 24:14; 25:31-33, 40; Mr 13:10";
        public const string MultipleCitationsResult = "Mt&nbsp;24:14; 25:31-33,&nbsp;40; Mr&nbsp;13:10";
        public const string MultipleCitations2 = "Ge 6:5; 8:21; Jas 1:14, 15";
        public const string FullNameCitation = "First John 5:3";
        public const string SongOfSolomon = "Song of Solomon 2:1";
        public const string SongOfSolomonResult = "Ca&nbsp;2:1";
    }
    class CitationHandlerTests
    {
        [Test]
        public async Task SimpleCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.SimpleCitation)[Ordinals.first];
            var writer = Writer.New();
            await CitationConverter.ToString(citation, writer);
            citationText = writer.Response();
            Assert.AreEqual(Citations.SimpleCitation.Replace(" ", "&nbsp;"), citationText);
        }

        [Test]
        public async Task CommaCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.CommaCitation)[Ordinals.first];
            var writer = Writer.New(); 
            await CitationConverter.ToString(citation, writer);
            citationText = writer.Response();
            Assert.AreEqual(Citations.CommaCitation.Replace(" ", HTMLTags.NonbreakingSpace), citationText);
        }

        [Test]
        public async Task BrokenCommaCitation()
        {
            string citationText;
            List<Citation> citations = CitationConverter.FromString(Citations.BrokenCommaCitation);
            var writer = Writer.New(); 
            await CitationConverter.ToString(citations, writer);
            citationText = writer.Response();
            Assert.AreEqual(Citations.BrokenCommaCitationResult, citationText);
        }

        [Test]
        public async Task FullNameCitation()
        {
            string citationText;
            List<Citation> citations = CitationConverter.FromString(Citations.FullNameCitation);
            var writer = Writer.New(); 
            await CitationConverter.ToString(citations, writer);
            citationText = writer.Response();
            Assert.AreEqual(Citations.NumberedBookCitationResult, citationText);
            citations = CitationConverter.FromString(Citations.SongOfSolomon);
            writer = Writer.New();
            await CitationConverter.ToString(citations, writer);
            citationText = writer.Response();
            Assert.AreEqual(Citations.SongOfSolomonResult, citationText);
        }

        [Test]
        public async Task ChapterCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.ChapterCitation)[Ordinals.first];
            var writer = Writer.New();
            await CitationConverter.ToString(citation, writer);
            citationText = writer.Response();
            Assert.AreEqual(CitationTypes.Chapter, citation.CitationType);
            HTMLWriter builder = Writer.New();
            IParagraph paragraph = new Paragraph()
            {
                Text = citationText
            };
            PageFormatter formatter = new PageFormatter(builder);
            paragraph.Text = Citations.ChapterCitation;
            CitationHandler citationHandler = new CitationHandler();
            StringRange range = new StringRange(0, Citations.ChapterCitation.Length - 1);
            var citations = citationHandler.ParseCitations(range, paragraph);
            await formatter.AppendCitationLabels(paragraph, citations);
            Assert.AreEqual(Citations.ChapterCitation, builder.Response());
        }

        [Test]
        public async Task NumberedBookCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.NumberedBookCitation)[Ordinals.first];
            var writer = Writer.New();
            await CitationConverter.ToString(citation, writer);
            citationText = writer.Response();
            Assert.AreEqual(Citations.NumberedBookCitationResult, citationText);
        }

        [Test]
        public async Task ShortBookCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.ShortBookCitation)[Ordinals.first];
            var writer = Writer.New();
            await CitationConverter.ToString(citation, writer);
            citationText = writer.Response();
            Assert.AreEqual(Citations.ShortBookCitation.Replace(" ", HTMLTags.NonbreakingSpace), citationText);
        }

        [Test]
        public async Task ShortBookCommaCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.ShortBookCommaCitation)[Ordinals.first];
            var writer = Writer.New();
            await CitationConverter.ToString(citation, writer);
            citationText = writer.Response();
            Assert.AreEqual(Citations.ShortBookCommaCitation.Replace(" ", HTMLTags.NonbreakingSpace), citationText);
        }

        [Test]
        public async Task ShortBookBrokenCitation()
        {
            string citationText;
            List<Citation> citations = CitationConverter.FromString(Citations.ShortBookBrokenCitation);
            var writer = Writer.New();
            await CitationConverter.ToString(citations, writer);
            citationText = writer.Response();
            Assert.AreEqual(Citations.ShortBookBrokenCitation.Replace(" ", HTMLTags.NonbreakingSpace), citationText);
        }

        [Test]
        public async Task MultipleCitation()
        {
            string citationText;
            List<Citation> citation = CitationConverter.FromString(Citations.MultipleCitations);
            var writer = Writer.New();
            await CitationConverter.ToString(citation, writer);
            citationText = writer.Response();
            Assert.AreEqual(Citations.MultipleCitationsResult, citationText);
            HTMLWriter builder = Writer.New();
            IParagraph paragraph = new Paragraph()
            {
                Text = citationText
            };
            PageFormatter formatter = new PageFormatter(builder);
            paragraph.Text = Citations.MultipleCitations;
            CitationHandler citationHandler = new CitationHandler();
            StringRange range = new StringRange(0, Citations.MultipleCitations.Length - 1);
            var citations = citationHandler.ParseCitations(range, paragraph);
            await formatter.AppendCitationLabels(paragraph, citations);
            Assert.AreEqual(Citations.MultipleCitations, builder.Response()); ;
        }

        [Test]
        public async Task RangeCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.RangeCitation)[Ordinals.first];
            var writer = Writer.New();
            await CitationConverter.ToString(citation, writer);
            citationText = writer.Response();
            Assert.AreEqual(Citations.RangeCitation.Replace(" ", HTMLTags.NonbreakingSpace), citationText);
        }

        [Test]
        public async Task BangCitation()
        {
            string citationText;
            Citation citation = CitationConverter.FromString(Citations.BangCitation)[Ordinals.first];
            var writer = Writer.New();
            await CitationConverter.ToString(citation, writer);
            citationText = writer.Response();
            Assert.That(citation.CitationType == CitationTypes.Verse);
            Assert.AreEqual(Citations.BangCitationResult, citationText);
        }
        private static void TestCitation(string citationText, Citation citation)
        {
            (List<Citation> citations, IParagraph paragraph) = SetupCitation(citationText);
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
                Assert.AreEqual(citationText.Replace(" ", HTMLTags.NonbreakingSpace), label);
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

        private static void TestCitations(string citationText, List<Citation> referenceCitations)
        {
            (List<Citation> citations, IParagraph paragraph) = SetupCitation(citationText);
            Assert.That(citations.Count > 0, () =>
            {
                return citationText;
            });
            if (citations.Count > 0)
            {
                for (int i = Ordinals.first; i < citations.Count; i++)
                {
                    int correctBook = referenceCitations[i].CitationRange.Book;
                    int correctChapter = referenceCitations[i].CitationRange.FirstChapter;
                    int correctVerse = referenceCitations[i].CitationRange.FirstVerse;
                    var firstCitation = citations[i];
                    var book = firstCitation.CitationRange.Book;
                    int chapter = firstCitation.CitationRange.FirstChapter;
                    int verse = firstCitation.CitationRange.FirstVerse;
                    Assert.That(book == correctBook && chapter == correctChapter &&
                        verse == correctVerse, () =>
                        {
                            return "book =" + book + "(" + correctBook + ") chapter=" + chapter + "(" +
                              correctChapter + ") verse=" + verse + "(" + correctVerse + ")";
                        });
                    Assert.That(firstCitation.Label.Start == 0);
                    string label = paragraph.StringAt(firstCitation.Label);
                    Assert.AreEqual(citationText, label);
                    correctChapter = referenceCitations[i].CitationRange.LastChapter;
                    correctVerse = referenceCitations[i].CitationRange.LastVerse;
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
        }

        private static (List<Citation> citations, IParagraph paragraph) SetupCitation(string textOfCitation)
        {

            CitationHandler citationHandler = new CitationHandler();
            IParagraph paragraph = new Paragraph()
            {
                Text = textOfCitation
            };
            paragraph.Text = textOfCitation;
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(0);
            mainRange.MoveEndTo(textOfCitation.Length - 1);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
            return (citations, paragraph);

        }


    }
}
