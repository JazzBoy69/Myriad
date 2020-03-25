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

        abstract Span<char> SpanAt(int start, int end);

        abstract Span<char> SpanAt(StringRange range);

        abstract int IndexOf(char token, int start, int end);

    }
    public class MarkedUpParagraph : IMarkedUpParagraph
    {
        char[] text;
        public MarkedUpParagraph()
        {
        }
        public int Length { get { return text.Length; } }

        public string StringAt(StringRange range)
        {
            return StringAt(range.Start, range.End);
        }
        
        public string Text 
        { 
            get 
            { 
                return text.ToString(); 
            } 
            set 
            { 
                text = value.ToArray();
            } 
        }

        public Span<char> TextSpan
        {
            get
            {
                return new Span<char>(text);
            }
        }
        public int IndexOfAny(char[] tokens, int start)
        {
            var textSpan = TextSpan;
            return textSpan.Slice(start, textSpan.Length-start).IndexOfAny(tokens)+start;
        }

        public int IndexOf(char token, int start)
        {
            var textSpan = TextSpan;
            return textSpan.Slice(start, textSpan.Length - start).IndexOf(token)+start;
        }

        public char CharAt(int index)
        {
            if (index >= text.Length) return (char)0;
            return text[index];
        }

        public string StringAt(int start, int end)
        {
            if ((start<0) || (end>=Length) || (end<start)) return "";
            return TextSpan.Slice(start, end-start+1).ToString();
        }
        
        public int IndexOf(char token, int start, int end)
        {
            return TextSpan.Slice(start, end - start).IndexOf(token);
        }

        public int TokenAt(int index)
        {
            return text[index] * 256 + text[index + 1];
        }

        public Span<char> SpanAt(int start, int end)
        {
            if ((start < 0) || (end >= Length) || (end < start)) return Span<char>.Empty;
            return new Span<char>(text, start, end - start + 1);
        }

        public Span<char> SpanAt(StringRange range)
        {
            return SpanAt(range.Start, range.End);
        }
    }
}
