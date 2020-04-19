﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Myriad.Library;
using Myriad.CitationHandlers;

namespace Myriad.Parser
{
    public class CitationConverter
    {
        public static List<Citation> FromString(string stringToConvert)
        {
            var citationHandler = new QueryCitationHandler();
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
            await Append(writer, citation, false);
        }

        public static async Task ToString(List<Citation> citations, HTMLWriter writer)
        {
            for (var i = Ordinals.first; i < citations.Count; i++)
            {
                if (i == Ordinals.first) await Append(writer, citations[i], false);
                else
                    await AppendNext(writer, citations[i - 1], citations[i]);
            }
        }

        public static async Task<string> ToLongString(Citation citation)
        {
            var writer = Writer.New();
            await ToLongString(citation, writer);
            return writer.Response();
        }

        public static async Task<string> ToLongString(List<Citation> citations)
        {
            var writer = Writer.New();
            await ToLongString(citations, writer);
            return writer.Response();
        }

        public static async Task ToLongString(Citation citation, HTMLWriter writer)
        {
            await Append(writer, citation, true);
        }

        public static async Task ToLongString(List<Citation> citations, HTMLWriter writer)
        {
            for (var i = Ordinals.first; i < citations.Count; i++)
            {
                if (i == Ordinals.first) await Append(writer, citations[i], true);
                else
                    await AppendNext(writer, citations[i - 1], citations[i]);
            }
        }

        internal async static Task Append(HTMLWriter writer, Citation citation, bool longName)
        {
            if ((citation.CitationRange.Book < Ordinals.first) ||
                (citation.CitationRange.Book >= Bible.Abbreviations.Count)) return;
            if (longName)
            {
                await writer.Append(Bible.NamesTitleCase[citation.CitationRange.Book]);
            }
            else
            {
                await writer.Append(Bible.AbbreviationsTitleCase[citation.CitationRange.Book]);
            }
            await writer.Append(HTMLTags.NonbreakingSpace);
            if (!Bible.IsShortBook(citation.CitationRange.Book))
            {
                await writer.Append(citation.CitationRange.FirstChapter);
                if (citation.CitationType != CitationTypes.Chapter)
                    await writer.Append(":");
            }
            if (citation.CitationType == CitationTypes.Chapter) return;
            await writer.Append(citation.CitationRange.FirstVerse);
            if (citation.CitationType == CitationTypes.Verse) return;
            if (!citation.CitationRange.IsOneVerse)
            {
                if ((citation.CitationRange.FirstChapter == citation.CitationRange.LastChapter) &&
                    (citation.CitationRange.FirstVerse + 1 == citation.CitationRange.LastVerse))
                    await writer.Append(","+HTMLTags.NonbreakingSpace);
                else
                    await writer.Append("-");
                if (!citation.CitationRange.OneChapter)
                {
                    await writer.Append(citation.CitationRange.LastChapter);
                    await writer.Append(":");
                }
                await writer.Append(citation.CitationRange.LastVerse);
            }
        }

        public static async Task AppendLink(HTMLWriter writer, Citation citation)
        {
            await PageFormatter.StartCitationAnchor(writer, citation);
            await Append(writer, citation, false);
            await writer.Append(HTMLTags.EndAnchor);
        }
        private static async Task AppendNext(HTMLWriter writer, Citation precedingCitation, Citation currentCitation)
        {
            if (precedingCitation.CitationRange.Book != currentCitation.CitationRange.Book)
            {
                await writer.Append("; ");
                await Append(writer, currentCitation, false);
                return;
            }
            else
            if (precedingCitation.CitationRange.LastChapter != currentCitation.CitationRange.FirstChapter)
            {
                await writer.Append("; ");
                await writer.Append(currentCitation.CitationRange.FirstChapter);
                await writer.Append(":");
            }
            else
            {
                await writer.Append(","+HTMLTags.NonbreakingSpace);
            }
            await writer.Append(currentCitation.CitationRange.FirstVerse);
            if (!currentCitation.CitationRange.IsOneVerse)
            {
                if ((currentCitation.CitationRange.FirstChapter == currentCitation.CitationRange.LastChapter) &&
                    (currentCitation.CitationRange.FirstVerse + 1 == currentCitation.CitationRange.LastVerse))
                    await writer.Append(", ");
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
    }
}
