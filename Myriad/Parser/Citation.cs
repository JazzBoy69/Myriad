using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Parser
{
    public enum CitationTypes
    {
        Chapter, Text, Verse, Invalid
    }
    public class Citation
    {
        public StringRange Label = new StringRange();
        public StringRange LeadingSymbols = new StringRange();
        public StringRange TrailingSymbols = new StringRange();
        public CitationRange CitationRange = CitationRange.InvalidRange;
        public CitationTypes CitationType = CitationTypes.Invalid;

        public Citation()
        {

        }
        public Citation(int book, int chapter, int verse)
        {
            CitationRange = new CitationRange(book, chapter, verse);
            CitationType = CitationTypes.Text;
        }
        public static Citation InvalidCitation
        {
            get
            {
                var citation = new Citation(-1, -1, -1);
                citation.CitationType = CitationTypes.Invalid;
                return citation;
            }
        }

        internal Citation Copy()
        {
            Citation newCitation = new Citation();
            newCitation.Label = new StringRange(Label.Start, Label.End);
            newCitation.CitationRange = new CitationRange(CitationRange.StartID,
                CitationRange.EndID);
            newCitation.CitationType = CitationType;
            newCitation.LeadingSymbols = new StringRange(LeadingSymbols.Start, LeadingSymbols.End);
            newCitation.TrailingSymbols = new StringRange(TrailingSymbols.Start, TrailingSymbols.End);
            return newCitation;
        }
    }

    public class VerseReference
    {
        public int Book = Result.notfound;
        public int Chapter = Result.notfound;
        public int Verse = Result.notfound;
        public int WordIndex = Result.notfound;
        public void Reset()
        {
            Book = Result.notfound;
            Chapter = Result.notfound;
            Verse = Result.notfound;
            WordIndex = Result.notfound;
        }
    }

}
