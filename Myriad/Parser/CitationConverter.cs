using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Myriad.Library;

namespace Myriad.Parser
{
    public class CitationConverter
    {
        public static List<Citation> FromString(string stringToConvert)
        {
            var citationHandler = new QueryCitationHandler();
            MarkedUpParagraph paragraph = new MarkedUpParagraph();
            paragraph.Text = stringToConvert;
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(Ordinals.first);
            mainRange.MoveEndTo(stringToConvert.Length-1);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
            return (citations.Count>Number.nothing) ? 
                citations :
                new List<Citation>() { Citation.InvalidCitation };
        }

        public static string ToString(Citation citation)
        {
            HTMLStringWriter writer = new HTMLStringWriter();
            Append(writer, citation);
            return writer.Response();
        }

        public static string ToString(List<Citation> citations)
        {
            HTMLStringWriter writer = new HTMLStringWriter();
            for (var i = Ordinals.first; i < citations.Count; i++)
            {
                if (i == Ordinals.first) Append(writer, citations[i]);
                else
                    AppendNext(writer, citations[i - 1], citations[i]);
            }
            return writer.Response();
        }

        internal static void Append(HTMLWriter writer, Citation citation)
        {
            writer.Append(Bible.AbbreviationsTitleCase[citation.CitationRange.Book]);
            writer.Append(" ");
            if (!Bible.IsShortBook(citation.CitationRange.Book))
            {
                writer.Append(citation.CitationRange.FirstChapter);
                if (citation.CitationType != CitationTypes.Chapter)
                    writer.Append(":");
            }
            if (citation.CitationType == CitationTypes.Chapter) return;
            writer.Append(citation.CitationRange.FirstVerse);
            if (citation.CitationType == CitationTypes.Verse) return;
            if (!citation.CitationRange.IsOneVerse)
            {
                if ((citation.CitationRange.FirstChapter == citation.CitationRange.LastChapter) &&
                    (citation.CitationRange.FirstVerse + 1 == citation.CitationRange.LastVerse))
                    writer.Append(", ");
                else
                    writer.Append("-");
                if (!citation.CitationRange.OneChapter)
                {
                    writer.Append(citation.CitationRange.LastChapter);
                    writer.Append(":");
                }
                writer.Append(citation.CitationRange.LastVerse);
            }
        }

        private static void AppendNext(HTMLStringWriter writer, Citation precedingCitation, Citation currentCitation)
        {
            if (precedingCitation.CitationRange.Book != currentCitation.CitationRange.Book)
            {
                writer.Append("; ");
                Append(writer, currentCitation);
                return;
            }
            else
            if (precedingCitation.CitationRange.LastChapter != currentCitation.CitationRange.FirstChapter)
            {
                writer.Append("; ");
                writer.Append(currentCitation.CitationRange.FirstChapter);
                writer.Append(":");
            }
            else
            {
                writer.Append(", ");
            }
            writer.Append(currentCitation.CitationRange.FirstVerse);
            if (!currentCitation.CitationRange.IsOneVerse)
            {
                if ((currentCitation.CitationRange.FirstChapter == currentCitation.CitationRange.LastChapter) &&
                    (currentCitation.CitationRange.FirstVerse + 1 == currentCitation.CitationRange.LastVerse))
                    writer.Append(", ");
                else
                    writer.Append("-");
                if (!currentCitation.CitationRange.OneChapter)
                {
                    writer.Append(currentCitation.CitationRange.LastChapter);
                    writer.Append(":");
                }
                writer.Append(currentCitation.CitationRange.LastVerse);
            }
        }
    }
}
