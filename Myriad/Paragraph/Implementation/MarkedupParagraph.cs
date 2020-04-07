using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Paragraph.Implementation
{
    //todo move to Feliciana Library
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
                return new string(text);
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
            int result = TextSpan.Slice(start).IndexOfAny(tokens);
            return (result == Result.notfound) ?
                Result.notfound :
                result + start;
        }

        public int IndexOf(char token, int start)
        {
            int result = TextSpan.Slice(start).IndexOf(token);
            return (result == Result.notfound) ?
                Result.notfound :
                result + start;
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
            int result = TextSpan.Slice(start, end - start+1).IndexOf(token)+start;
            return (result == Result.notfound) ?
                Result.notfound :
                result + start;
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

        public int LastIndexOf(char token)
        {
            return Array.LastIndexOf(text, token);
        }
    }
}
