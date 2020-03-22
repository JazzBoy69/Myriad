using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Parser
{
    public interface IMarkedUpParagraph
    {
        public int Length { get; }

        public string Text { get ; set; }

        abstract int IndexOfAny(char[] tokens, int start);

        abstract int IndexOf(char token, int start);

        abstract char CharAt(int index);

        abstract int TokenAt(int index);

        abstract string StringAt(int start, int end);

        abstract string StringAt(StringRange range);

        abstract int IndexOf(char token, int start, int end);

    }
    public class MarkedUpParagraph : IMarkedUpParagraph //Todo: implement sliced version
    {
        string text;
        public MarkedUpParagraph()
        {
        }
        public int Length { get { return text.Length; } }

        public string StringAt(StringRange range)
        {
            return StringAt(range.Start, range.End);
        }

        public string Text { get { return text; } set { text = value; } }

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
            if ((start<0) || (end>=Length) || (end<=start)) return "";
            return text.Substring(start, end-start);
        }

        public int IndexOf(char token, int start, int end)
        {
            return StringAt(start, end).IndexOf(token) + start;
        }

        public int TokenAt(int index)
        {
            return text[index] * 256 + text[index + 1];
        }
    }
}
