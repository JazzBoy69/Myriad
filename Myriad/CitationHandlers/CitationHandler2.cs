using System;
using System.Collections.Generic;
using Feliciana.Library;
using Myriad.Library;

namespace Myriad.CitationHandlers
{
    public class CitationHandler2
    {
        Citation citation = new Citation();
        StringRange rangeToParse;
        List<Citation> results;
        IParagraph paragraphToParse;
        int count;
        public List<Citation> ParseCitations(StringRange givenRange, IParagraph givenParagraph)
        {
            try
            {
                InitializeParser(givenRange, givenParagraph);
                while (citation.Label.End <= rangeToParse.End)
                {
                    bool success = true;
                    if (!success) break;
                }
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public void InitializeParser(StringRange givenRange, IParagraph givenParagraph)
        {
            rangeToParse = givenRange;
            paragraphToParse = givenParagraph;
            char lastChar = givenParagraph.CharAt(rangeToParse.End);
            if (lastChar == '.') rangeToParse.PullEnd();
            results = new List<Citation>();
            citation = new Citation();
            citation.Label.MoveStartTo(rangeToParse.Start);
            citation.Label.MoveEndTo(rangeToParse.Start);
            count = Number.nothing;
        }

    }
}
