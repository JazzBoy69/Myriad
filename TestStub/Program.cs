using System;
using Myriad.Parser;

namespace TestStub
{
    class Program
    {
        static void Main(string[] args)
        {
            string textOfCitation = "(1Jo 5:3)";
            CitationHandler citationHandler = new CitationHandler();
            MarkedUpParagraph paragraph = new MarkedUpParagraph();
            paragraph.Text = textOfCitation;
            StringRange mainRange = new StringRange();
            mainRange.MoveStartTo(1);
            mainRange.MoveEndTo(textOfCitation.Length - 1);
            var citations = citationHandler.ParseCitations(mainRange, paragraph);
        }
    }
}
