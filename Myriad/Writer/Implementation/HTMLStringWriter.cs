using System;
using System.Text;
using System.Threading.Tasks;
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
        public async Task StartSpanWithClass(string className)
        {
            await Task.Run(() =>
            {
                writer.Append(HTMLTags.StartSpanWithClass);
                writer.Append(className);
                writer.Append(HTMLTags.CloseQuoteEndTag);
            });
        }

        public async Task StartAnchorWithClass(string className)
        {
            await Task.Run(() =>
            {
                writer.Append(HTMLTags.StartAnchorWithClass);
                writer.Append(className);
                writer.Append(HTMLTags.CloseQuote);
            });
        }

        public async Task Append(char c)
        {
            await Task.Run(() => writer.Append(c));
        }

        public async Task AppendHREF(string pageName)
        {
            await Task.Run(() =>
            {
                writer.Append(HTMLTags.HREF);
                writer.Append(pageName);
            });
        }

        public async Task Append(string stringToAppend)
        {
            await Task.Run(() => writer.Append(stringToAppend));
        }

        public async Task Append(int number)
        {
            await Task.Run(() => writer.Append(number));
        }

        public async Task StartDivWithClass(string className)
        {
            await Task.Run(() =>
            {
                writer.Append(HTMLTags.StartDivWithClass);
                writer.Append(className);
                writer.Append(HTMLTags.CloseQuoteEndTag);
            });
        }

        public async Task StartDivWithID(string id)
        {
            await Task.Run(() =>
            {
                writer.Append(HTMLTags.StartDivWithID);
                writer.Append(id);
                writer.Append(HTMLTags.CloseQuoteEndTag);
            });
        }

        public async Task StartFigure(string className)
        {
            await Task.Run(() =>
            {
                writer.Append(HTMLTags.StartFigureWithClass);
                writer.Append(className);
                writer.Append(HTMLTags.CloseQuoteEndTag);
            });
        }

        public async Task StartIMG(string path)
        {
            await Task.Run(() =>
            {
                writer.Append(HTMLTags.StartImg);
                writer.Append(path);
                writer.Append(HTMLTags.CloseQuote);
            });
        }

        public async Task AppendIMGWidth(string widthString)
        {
            await Task.Run(() =>
            {
                writer.Append(HTMLTags.Width);
                writer.Append(widthString);
                writer.Append(HTMLTags.CloseQuote);
            });
        }

        public async Task AppendClass(string className)
        {
            await Task.Run(() =>
            {
                writer.Append(HTMLTags.Class);
                writer.Append(className);
                writer.Append(HTMLTags.CloseQuote);
            });
        }

        public async Task StartParagraphWithClass(string className)
        {
            await Task.Run(() =>
            {
                writer.Append(HTMLTags.StartParagraphWithClass);
                writer.Append(className);
                writer.Append(HTMLTags.CloseQuoteEndTag);
            });
        }

        public async Task StartSectionWithClass(string className)
        {
            await Task.Run(() =>
            {
                writer.Append(HTMLTags.StartSectionWithClass);
                writer.Append(className);
                writer.Append(HTMLTags.CloseQuoteEndTag);
            });
        }
    }
}