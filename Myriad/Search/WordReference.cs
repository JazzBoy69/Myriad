using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Search
{
    public class WordReference
    {
        readonly CitationRange range;
        readonly int wordindex;
        readonly int sentenceID;
        public WordReference(CitationRange range, int wordindex, int sentenceID)
        {

            this.range = range;
            this.wordindex = wordindex;
            this.sentenceID = sentenceID;
        }

        public CitationRange Range
        {
            get { return range; }
        }

        public int WordIndex
        {
            get { return wordindex; }
        }

        public int SentenceID
        {
            get { return sentenceID; }
        }
    }
}
