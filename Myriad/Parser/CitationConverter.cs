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
            StringBuilder result = new StringBuilder(
                Bible.NamesTitleCase[citation.CitationRange.Book]);
            result.Append(" ");
            if (!Bible.IsShortBook(citation.CitationRange.Book))
            {
                result.Append(citation.CitationRange.FirstChapter);
                if (citation.CitationType != CitationTypes.Chapter)
                    result.Append(":");
            }
            if (citation.CitationType == CitationTypes.Chapter) return result.ToString();
            result.Append(citation.CitationRange.FirstVerse);
            if (citation.CitationType == CitationTypes.Verse) return result.ToString();
            if (!citation.CitationRange.IsOneVerse)
            {
                result.Append("-");
                if (!citation.CitationRange.OneChapter)
                {
                    result.Append(citation.CitationRange.LastChapter);
                    result.Append(":");
                }
                result.Append(citation.CitationRange.LastVerse);
            }
            return result.ToString();
        }
    }
}
