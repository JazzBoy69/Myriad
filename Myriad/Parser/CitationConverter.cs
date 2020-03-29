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
            return (citations.Count>Numbers.nothing) ? 
                citations :
                new List<Citation>() { Citation.InvalidCitation };
        }

        public static string ToString(Citation citation)
        {
            HTMLStringBuilder builder = new HTMLStringBuilder();
            Append(builder, citation);
            return builder.Response();
        }

        public static string ToString(List<Citation> citations)
        {
            HTMLStringBuilder builder = new HTMLStringBuilder();
            for (var i = Ordinals.first; i < citations.Count; i++)
            {
                if (i == Ordinals.first) Append(builder, citations[i]);
                else
                    AppendNext(builder, citations[i - 1], citations[i]);
            }
            return builder.Response();
        }

        internal static void Append(HTMLResponse builder, Citation citation)
        {
            builder.Append(Bible.AbbreviationsTitleCase[citation.CitationRange.Book]);
            builder.Append(" ");
            if (!Bible.IsShortBook(citation.CitationRange.Book))
            {
                builder.Append(citation.CitationRange.FirstChapter);
                if (citation.CitationType != CitationTypes.Chapter)
                    builder.Append(":");
            }
            if (citation.CitationType == CitationTypes.Chapter) return;
            builder.Append(citation.CitationRange.FirstVerse);
            if (citation.CitationType == CitationTypes.Verse) return;
            if (!citation.CitationRange.IsOneVerse)
            {
                if ((citation.CitationRange.FirstChapter == citation.CitationRange.LastChapter) &&
                    (citation.CitationRange.FirstVerse + 1 == citation.CitationRange.LastVerse))
                    builder.Append(", ");
                else
                    builder.Append("-");
                if (!citation.CitationRange.OneChapter)
                {
                    builder.Append(citation.CitationRange.LastChapter);
                    builder.Append(":");
                }
                builder.Append(citation.CitationRange.LastVerse);
            }
        }

        private static void AppendNext(HTMLStringBuilder builder, Citation precedingCitation, Citation currentCitation)
        {
            if (precedingCitation.CitationRange.Book != currentCitation.CitationRange.Book)
            {
                builder.Append("; ");
                Append(builder, currentCitation);
                return;
            }
            else
            if (precedingCitation.CitationRange.LastChapter != currentCitation.CitationRange.FirstChapter)
            {
                builder.Append("; ");
                builder.Append(currentCitation.CitationRange.FirstChapter);
                builder.Append(":");
            }
            else
            {
                builder.Append(", ");
            }
            builder.Append(currentCitation.CitationRange.FirstVerse);
            if (!currentCitation.CitationRange.IsOneVerse)
            {
                if ((currentCitation.CitationRange.FirstChapter == currentCitation.CitationRange.LastChapter) &&
                    (currentCitation.CitationRange.FirstVerse + 1 == currentCitation.CitationRange.LastVerse))
                    builder.Append(", ");
                else
                    builder.Append("-");
                if (!currentCitation.CitationRange.OneChapter)
                {
                    builder.Append(currentCitation.CitationRange.LastChapter);
                    builder.Append(":");
                }
                builder.Append(currentCitation.CitationRange.LastVerse);
            }
        }
    }
}
