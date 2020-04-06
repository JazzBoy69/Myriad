using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Writer.Implementation
{
    public class HTMLResponseWriter : HTMLWriter
    {
        private readonly HttpResponse response;

        public HTMLResponseWriter(HttpResponse response)
        {
            this.response = response;
        }

        public async Task Append(char c)
        {
            await response.WriteAsync(c.ToString());
        }

        public async Task Append(string stringToAppend)
        {
            await response.WriteAsync(stringToAppend);
        }

        public async Task Append(int number)
        {
            await response.WriteAsync(number.ToString());
        }

        public async Task AppendClass(string className)
        {
            await response.WriteAsync(HTMLTags.Class);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuote);
        }

        public async Task AppendHREF(string pageName)
        {
            await response.WriteAsync(HTMLTags.HREF);
            await response.WriteAsync(pageName);
        }

        public async Task AppendIMGWidth(string widthString)
        {
            await response.WriteAsync(HTMLTags.Width);
            await response.WriteAsync(widthString);
            await response.WriteAsync(HTMLTags.CloseQuote);
        }

        public string Response()
        {
            return "";
        }

        public async Task StartAnchorWithClass(string className)
        {
            await response.WriteAsync(HTMLTags.StartAnchorWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuote);
        }

        public async Task StartSectionWithClass(string className)
        {
            await response.WriteAsync(HTMLTags.StartSectionWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }

        public async Task StartDivWithClass(string className)
        {
            await response.WriteAsync(HTMLTags.StartDivWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }

        public async Task StartDivWithID(string id)
        {
            await response.WriteAsync(HTMLTags.StartDivWithID);
            await response.WriteAsync(id);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }

        public async Task StartFigure(string className)
        {
            await response.WriteAsync(HTMLTags.StartFigureWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }

        public async Task StartIMG(string path)
        {
            await response.WriteAsync(HTMLTags.StartImg);
            await response.WriteAsync(path);
            await response.WriteAsync(HTMLTags.CloseQuote);
        }

        public async Task StartSpanWithClass(string className)
        {
            await response.WriteAsync(HTMLTags.StartSpanWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }

        public async Task StartParagraphWithClass(string className)
        {
            await response.WriteAsync(HTMLTags.StartParagraphWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }
    }
}