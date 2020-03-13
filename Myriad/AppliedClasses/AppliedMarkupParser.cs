using Myriad.ToApplied;
using System;
using System.Collections.Generic;
using System.Text;

namespace Myriad.AppliedClasses
{
    internal class AppliedMarkupParser : MarkupParser
    {
        HTMLStringBuilder builder = new HTMLStringBuilder();
        public StringBuilder ParsedText { get { return builder.Builder; } }

        public void Parse(List<string> markupParagraphs)
        {
            foreach (string paragraph in markupParagraphs)
            {
                StartParagraph();
                Parse(paragraph);
                EndParagraph();
            }
        }

        private void EndParagraph()
        {
            builder.EndSpan();
            builder.EndParagraph();
        }

        private void StartParagraph()
        {
            builder.StartSection();
            builder.StartParagraph();
            builder.StartParagraphContent();
        }

        private void Parse(string paragraph)
        {

        }
    }
}