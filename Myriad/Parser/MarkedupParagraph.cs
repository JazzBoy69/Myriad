using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Parser
{
    public class MarkedupParagraphList<T> where T: MarkedUpParagraph, new()
    {
        public static List<T> CreateFrom(List<string> paragraphs)
        {
            List<T> result = new List<T>();
            foreach (string paragraph in paragraphs)
            {
                var markedupParagraph = new T();
                markedupParagraph.Text = paragraph;
            }
            return result;
        }
    }




    public class MarkedUpParagraph
    {
        string text;
        public int Length { get { return text.Length; } }

        public string Text { get { return text; } internal set { text = value; } }

        public int IndexOfAny(char[] tokens, int start)
        {
            return text.IndexOfAny(tokens, start);
        }

        public int IndexOf(char token, int start)
        {
            return text.IndexOf(token, start);
        }

        public char CharAt(int index)
        {
            if (index >= text.Length) return (char)0;
            return text[index];
        }

        public string StringAt(int start, int end)
        {
            return text[start..end];
        }

        public int IndexOf(char token, int start, int end)
        {
            return StringAt(start, end).IndexOf(token) + start;
        }
    }
}
