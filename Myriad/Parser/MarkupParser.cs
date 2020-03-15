using System;
using System.Collections.Generic;
using System.Text;
using Myriad.Data;

namespace Myriad.Parser

{
    public class MarkedUpParagraph
    {
        private string text;
        public MarkedUpParagraph(string text)
        {
            this.text = text;
        }
    }
    public class MarkupParser
    {
        HTMLStringBuilder builder = new HTMLStringBuilder();
        public StringBuilder ParsedText { get { return builder.Builder; } }

        public void Parse(List<MarkedUpParagraph> paragraphs)
        {
        }

    }
}