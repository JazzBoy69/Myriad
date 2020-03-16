using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Parser
{
    public class MarkedUpParagraph
    {
        private readonly string text;
        public MarkedUpParagraph(string text)
        {
            this.text = text;
        }

        public int Length { get { return text.Length; } }

        public string Text { get { return text; } }

        internal static List<MarkedUpParagraph> CreateFrom(List<string> paragraphs)
        {
            List<MarkedUpParagraph> result = new List<MarkedUpParagraph>();
            foreach (string paragraph in paragraphs)
            {
                result.Add(new MarkedUpParagraph(paragraph));
            }
            return result;
        }

        internal int IndexOfAny(char[] tokens, int start)
        {
            return text.IndexOfAny(tokens, start);
        }

        internal int IndexOf(char token, int start)
        {
            return text.IndexOf(token, start);
        }

        internal char CharAt(int index)
        {
            if (index >= text.Length) return (char)0;
            return text[index];
        }

        internal string StringAt(int start, int end)
        {
            return text[start..end];
        }
    }
}
