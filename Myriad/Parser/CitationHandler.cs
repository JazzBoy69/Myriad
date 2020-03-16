using System;

namespace Myriad.Parser
{
    internal class CitationHandler
    {
        private StringRange mainRange;
        private MarkedUpParagraph currentParagraph;

        public CitationHandler(StringRange mainRange, MarkedUpParagraph currentParagraph)
        {
            this.mainRange = mainRange;
            this.currentParagraph = currentParagraph;
        }

        internal void AppendCitations()
        {
            //Todo: implement append citations
            throw new NotImplementedException();
        }
    }
}