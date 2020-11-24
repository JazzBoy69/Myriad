using System.Collections.Generic;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Myriad.Library;
using Myriad.CitationHandlers;
using Myriad.Data;
using Myriad.Pages;
using System;
using Feliciana.Data;

namespace Myriad.Parser
{
    public class CitationConverter
    {
        public static List<Citation> FromString(string stringToConvert)
        {
            if (string.IsNullOrEmpty(stringToConvert)) return new List<Citation>() { Citation.InvalidCitation };
            var citationHandler = new QueryCitationHandler2();
            IParagraph paragraph = new Paragraph()
            {
                Text = stringToConvert
            };
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(Ordinals.first);
            mainRange.MoveEndTo(stringToConvert.Length-1);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
            return (citations.Count>Number.nothing) ? 
                citations :
                new List<Citation>() { Citation.InvalidCitation };
        }

        public static async Task<string> ToString(Citation citation)
        {
            var writer = Writer.New();
            await ToString(citation, writer);
            return writer.Response();
        }

        public static async Task<string> ToString(List<Citation> citations)
        {
            var writer = Writer.New();
            await ToString(citations, writer);
            return writer.Response();
        }

        public static async Task ToString(Citation citation, HTMLWriter writer)
        {
            await Append(writer, citation);
        }

        public static async Task ToString(List<Citation> citations, HTMLWriter writer)
        {
            for (var i = Ordinals.first; i < citations.Count; i++)
            {
                if (i == Ordinals.first) await Append(writer, citations[i]);
                else
                {
                    await AppendConnectingPunctuation(writer, citations[i - 1], citations[i]);
                    await AppendNext(writer, citations[i - 1], citations[i]);
                }
            }
        }


        internal static async Task AppendChapterTitle(HTMLWriter writer, CitationRange citationRange)
        {      
            await writer.Append(Bible.AbbreviationsTitleCase[citationRange.Book]);
            await writer.Append("_");
            await writer.Append(citationRange.FirstChapter);
        }


        internal async static Task Append(HTMLWriter writer, Citation citation)
        {
            if ((citation.CitationRange.Book < Ordinals.first) ||
                (citation.CitationRange.Book >= Bible.Abbreviations.Count)) return;

            await writer.Append(Bible.Names[citation.LabelType][citation.CitationRange.Book]);
            await writer.Append(HTMLTags.NonbreakingSpace);
            if (!Bible.IsShortBook(citation.CitationRange.Book))
            {
                await writer.Append(citation.CitationRange.FirstChapter);
                if ((citation.CitationType != CitationTypes.Chapter) &&
                    (citation.CitationType != CitationTypes.ChapterRange))
                    await writer.Append(":");
            }
            if (citation.CitationType == CitationTypes.Chapter) return;
            if (citation.CitationType != CitationTypes.ChapterRange)
            {
                if (citation.CitationRange.FirstVerse == 0)
                {
                    if ((citation.CitationRange.Book != 18) || (!Bible.ChaptersWithSuperscription.Contains(
                            citation.CitationRange.FirstChapter)))
                        citation.CitationRange.SetFirstVerse(1);
                }
                if (citation.CitationRange.FirstVerse == 0)
                    await writer.Append("Sup");
                else
                    await writer.Append(citation.CitationRange.FirstVerse);
            }
            if (citation.CitationType == CitationTypes.Verse) return;
            if (citation.CitationRange.IsOneVerse) return;

            if ((citation.CitationRange.FirstChapter == citation.CitationRange.LastChapter) &&
                (citation.CitationRange.FirstVerse + 1 == citation.CitationRange.LastVerse))
            {
                await writer.Append("," + HTMLTags.NonbreakingSpace);
                await writer.Append(citation.CitationRange.LastVerse);
                return;
            }
            if (!citation.CitationRange.OneChapter)
            {
                if ((citation.CitationRange.LastChapter == citation.CitationRange.FirstChapter + 1) &&
                    (citation.CitationType == CitationTypes.ChapterRange))
                {
                    await writer.Append(",&nbsp;");
                }
                else
                if (citation.CitationType == CitationTypes.ChapterRange)
                    await writer.Append("-");
                else
                    await writer.Append("–");
                await writer.Append(citation.CitationRange.LastChapter);
                if (citation.CitationType == CitationTypes.ChapterRange) return;
                await writer.Append(":");
            }
            else
                await writer.Append("-");
            await writer.Append(citation.CitationRange.LastVerse);

        }


        public static async Task<List<Citation>> ResolveCitations(List<Citation> citations)
        {
            List<Citation> result = new List<Citation>();
            for (int index = Ordinals.first; index < citations.Count; index++)
            {   
                result.Add(await ResolveCitation(citations[index]));
            }
            return result;
        }

        public async static Task<Citation> ResolveCitation(Citation citation)
        {
            Citation newCitation = citation.Copy();
            if (citation.CitationRange.WordIndexIsDeferred)
            {
                newCitation.CitationRange.SetWordIndex(
                    await ReadDeferredWord(citation.CitationRange.Word,
                    citation.CitationRange.StartID.ID,
                    citation.CitationRange.EndID.ID)
                    );
            }
            if (citation.CitationRange.EndID.WordIndex == KeyID.MaxWordIndex)
            {
                newCitation.CitationRange.SetLastWordIndex(
                    await ReadLastWordIndex(citation.CitationRange.Book, citation.CitationRange.LastChapter,
                    citation.CitationRange.LastVerse));
            }
            return newCitation;
        }

        internal static List<CrossReference> ToCrossReferences(List<Citation> citations, int ID, int paragraphIndex)
        {
            List<CrossReference> result = new List<CrossReference>();
            for (int index = Ordinals.first; index < citations.Count; index++)
            {
                Citation citation = citations[index];
                result.Add(new CrossReference(ID, paragraphIndex, citations[index].CitationRange.StartID.ID,
                    citations[index].CitationRange.EndID.ID));
            }
            return result;
        }

        internal static async Task<int> ReadLastWordIndex(int book, int chapter, int verse)
        {
            var reader = new DataReaderProvider<int, int, int>(SqlServerInfo.GetCommand(DataOperation.ReadLastWordIndex),
                book, chapter, verse);
            int result = await reader.GetDatum<int>();
            reader.Close();
            return result;
        }

        public static async Task<int> ReadDeferredWord(string indexWord, int start, int end)
        {
            var reader = new DataReaderProvider<string, int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadWordIndex),
                indexWord, start, end);
            int result = await reader.GetDatum<int>();
            reader.Close();
            return result;
        }
        public static async Task AppendLink(HTMLWriter writer, Citation citation)
        {
            await PageFormatter.StartCitationLink(writer, citation);
            await Append(writer, citation);
            await writer.Append(HTMLTags.EndAnchor);
        }

        public static async Task AppendLink(HTMLWriter writer, Citation citation, Citation targetCitation)
        {
            await PageFormatter.StartCitationLink(writer, citation, targetCitation);
            await Append(writer, citation);
            await writer.Append(HTMLTags.EndAnchor);
        }

        public static async Task AppendLinks(HTMLWriter writer, List<Citation> citations, CitationRange targetRange)
        {
            for (var i = Ordinals.first; i < citations.Count; i++)
            {
                if (i == Ordinals.first)
                {
                    await PageFormatter.StartCitationAnchor(writer, citations[i], targetRange);
                    await Append(writer, citations[i]);
                    await writer.Append(HTMLTags.EndAnchor);
                    continue;
                }
                await AppendConnectingPunctuation(writer, citations[i - 1], citations[i]);
                await PageFormatter.StartCitationAnchor(writer, citations[i], targetRange);
                await AppendNext(writer, citations[i - 1], citations[i]);
                await writer.Append(HTMLTags.EndAnchor);
            }
        }

        public static async Task AppendLinks(HTMLWriter writer, List<Citation> citations)
        {
            for (var i = Ordinals.first; i < citations.Count; i++)
            {
                if (i == Ordinals.first)
                {
                    await PageFormatter.StartCitationLink(writer, citations[i]);
                    await Append(writer, citations[i]);
                    await writer.Append(HTMLTags.EndAnchor);
                    continue;
                }
                await AppendConnectingPunctuation(writer, citations[i - 1], citations[i]);
                await PageFormatter.StartCitationLink(writer, citations[i]);
                await AppendNext(writer, citations[i - 1], citations[i]);
                await writer.Append(HTMLTags.EndAnchor);
            }
        }
        private static async Task AppendNext(HTMLWriter writer, Citation precedingCitation, Citation currentCitation)
        {
            if (precedingCitation.CitationRange.Book != currentCitation.CitationRange.Book)
            {
                await Append(writer, currentCitation);
                return;
            }
            else
            if ((precedingCitation.CitationRange.LastChapter != currentCitation.CitationRange.FirstChapter) &&
                (currentCitation.CitationType != CitationTypes.Chapter))
            {
                await writer.Append(currentCitation.CitationRange.FirstChapter);
                await writer.Append(":");
            }
            if (currentCitation.CitationType == CitationTypes.Chapter)
            {
                await writer.Append(currentCitation.CitationRange.FirstChapter);
                return;
            }
            await writer.Append(currentCitation.CitationRange.FirstVerse);
            if (!currentCitation.CitationRange.IsOneVerse)
            {
                if ((currentCitation.CitationRange.FirstChapter == currentCitation.CitationRange.LastChapter) &&
                    (currentCitation.CitationRange.FirstVerse + 1 == currentCitation.CitationRange.LastVerse))
                    await writer.Append("," + HTMLTags.NonbreakingSpace);
                else
                    await writer.Append("-");
                if (!currentCitation.CitationRange.OneChapter)
                {
                    await writer.Append(currentCitation.CitationRange.LastChapter);
                    await writer.Append(":");
                }
                await writer.Append(currentCitation.CitationRange.LastVerse);
            }
        }

        private static async Task AppendConnectingPunctuation(HTMLWriter writer, Citation precedingCitation, Citation currentCitation)
        {
            if (precedingCitation.CitationRange.Book != currentCitation.CitationRange.Book)
            {
                await writer.Append("; ");
            }
            else
            if ((precedingCitation.CitationRange.LastChapter != currentCitation.CitationRange.FirstChapter) &&
                (currentCitation.CitationType != CitationTypes.Chapter))
            {
                await writer.Append("; ");
            }
            else
            {
                await writer.Append(", ");
            }
        }
    }
}
