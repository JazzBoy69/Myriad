using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}
