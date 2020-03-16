using System;

namespace Myriad.Parser
{
    internal class CitationHandler
    {
        private StringRange mainRange;
        private IMarkedUpParagraph currentParagraph;

        public CitationHandler(StringRange mainRange, IMarkedUpParagraph currentParagraph)
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