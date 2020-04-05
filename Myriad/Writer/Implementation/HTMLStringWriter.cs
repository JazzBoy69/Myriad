using System;
using System.Text;
using Myriad.Library;

namespace Myriad.Writer.Implementation
{
    public class HTMLStringWriter : HTMLWriter
    {
        readonly StringBuilder writer = new StringBuilder();
        private StringBuilder Writer => writer;

        public string Response()
        {
            return Writer.ToString();
        }
        public void StartSpanWithClass(string className)
        {
            writer.Append(HTMLTags.StartSpanWithClass);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuoteEndTag);
        }

        public void StartAnchorWithClass(string className)
        {
            writer.Append(HTMLTags.StartAnchorWithClass);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuote);
        }

        public void Append(char c)
        {
            writer.Append(c);
        }

        public void AppendHREF(string pageName)
        {
            writer.Append(HTMLTags.HREF);
            writer.Append(pageName);
        }

        public void Append(string stringToAppend)
        {
            writer.Append(stringToAppend);
        }

        public void Append(int number)
        {
            writer.Append(number);
        }

        public void StartDivWithClass(string className)
        {
            writer.Append(HTMLTags.StartDivWithClass);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuoteEndTag);
        }

        public void StartDivWithID(string id)
        {
            writer.Append(HTMLTags.StartDivWithID);
            writer.Append(id);
            writer.Append(HTMLTags.CloseQuoteEndTag);
        }

        public void StartFigure(string className)
        {
            writer.Append(HTMLTags.StartFigureWithClass);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuoteEndTag);        
        }

        public void StartIMG(string path)
        {
            writer.Append(HTMLTags.StartImg);
            writer.Append(path);
            writer.Append(HTMLTags.CloseQuote);
        }

        public void AppendIMGWidth(string widthString)
        {
            writer.Append(HTMLTags.Width);
            writer.Append(widthString);
            writer.Append(HTMLTags.CloseQuote);
        }

        public void AppendClass(string className)
        {
            writer.Append(HTMLTags.Class);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuote);
        }

        public void Append(ReadOnlySpan<char> span)
        {
            writer.Append(span);
        }

        public void StartParagraphWithClass(string className)
        {
            writer.Append(HTMLTags.StartParagraphWithClass);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuoteEndTag);
        }

        public void StartSectionWithClass(string className)
        {
            writer.Append(HTMLTags.StartSectionWithClass);
            writer.Append(className);
            writer.Append(HTMLTags.CloseQuoteEndTag);
        }
    }
}