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
        public static Citation FromString(string stringToConvert)
        {
            var citationHandler = new QueryCitationHandler();
            MarkedUpParagraph paragraph = new MarkedUpParagraph();
            paragraph.Text = stringToConvert;
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(Ordinals.first);
            mainRange.MoveEndTo(stringToConvert.Length - 1);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
            return (citations.Count>Numbers.nothing) ? 
                citations.First() :
                Citation.InvalidCitation;
        }

        public static string ToString(Citation citation)
        {
            HTMLStringBuilder builder = new HTMLStringBuilder();
            Append(builder, citation);
            return builder.Response();
        }

        internal static void Append(HTMLResponse builder, Citation citation)
        {
            builder.Append(Bible.NamesTitleCase[citation.CitationRange.Book]);
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
                builder.Append("-");
                if (!citation.CitationRange.OneChapter)
                {
                    builder.Append(citation.CitationRange.LastChapter);
                    builder.Append(":");
                }
                builder.Append(citation.CitationRange.LastVerse);
            }
        }
    }
}
