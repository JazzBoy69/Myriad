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

        internal static List<MarkedUpParagraph> Create(List<string> paragraphs)
        {
            List<MarkedUpParagraph> result = new List<MarkedUpParagraph>();
            foreach (string paragraph in paragraphs)
            {
                result.Add(new MarkedUpParagraph(paragraph));
            }
            return result;
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