using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Parser
{
    public class MarkedupParagraphList<T> where T: IMarkedUpParagraph
    {
        internal static List<T> CreateFrom(List<string> paragraphs)
        {
            List<T> result = new List<T>();
            foreach (string paragraph in paragraphs)
            {
                result.Add((T)Activator.CreateInstance(typeof(T), paragraph));
            }
            return result;
        }
    }


    public interface IMarkedUpParagraph
    {
        public string Text { get; }

        public int Length { get; }

        public abstract IMarkedUpParagraph Create(string text);
        public abstract int IndexOfAny(char[] tokens, int start);

        public abstract int IndexOf(char token, int start);

        public abstract char CharAt(int index);

        public abstract string StringAt(int start, int end);
    }

    public class MarkedUpParagraphString : IMarkedUpParagraph
    {
        string text;
        public int Length { get { return text.Length; } }

        public string Text { get { return text; } }

        public IMarkedUpParagraph Create(string text)
        {
            MarkedUpParagraphString newParagraph = new MarkedUpParagraphString();
            newParagraph.text = text;
            return newParagraph;
        }
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
    }
}
