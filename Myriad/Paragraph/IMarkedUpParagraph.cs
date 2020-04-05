using System;
using Myriad.Library;

namespace Myriad.Paragraph
{
    public interface IMarkedUpParagraph
    {
        public int Length { get; }

        public string Text { get; set; }

        abstract int IndexOfAny(char[] tokens, int start);

        abstract int IndexOf(char token, int start);

        abstract char CharAt(int index);

        abstract int TokenAt(int index);

        abstract string StringAt(int start, int end);

        abstract string StringAt(StringRange range);

        abstract Span<char> SpanAt(int start, int end);

        abstract Span<char> SpanAt(StringRange range);

        abstract int IndexOf(char token, int start, int end);
        int LastIndexOf(char token);
    }
}
