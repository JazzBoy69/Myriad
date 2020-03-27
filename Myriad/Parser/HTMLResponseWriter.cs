﻿using Myriad.Parser;
using Microsoft.AspNetCore.Http;
using System;

namespace Myriad.Parser
{
    public class HTMLResponseWriter : HTMLResponse
    {
        private HttpResponse response;

        public HTMLResponseWriter(HttpResponse response)
        {
            this.response = response;
        }

        public void Append(Span<char> span)
        {
            response.WriteAsync(span.ToString());
        }

        public async void Append(char c)
        {
            await response.WriteAsync(c.ToString());
        }

        public async void Append(string stringToAppend)
        {
            await response.WriteAsync(stringToAppend);
        }

        public async void Append(int number)
        {
            await response.WriteAsync(number.ToString());
        }

        public async void AppendClass(string className)
        {
            await response.WriteAsync(HTMLTags.Class);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuote);
        }

        public async void AppendHREF(string pageName)
        {
            await response.WriteAsync(HTMLTags.HREF);
            await response.WriteAsync(pageName);
        }

        public async void AppendIMGWidth(string widthString)
        {
            await response.WriteAsync(HTMLTags.Width);
            await response.WriteAsync(widthString);
            await response.WriteAsync(HTMLTags.CloseQuote);
        }

        public string Response()
        {
            return "";
        }

        public async void StartAnchor(string className)
        {
            await response.WriteAsync(HTMLTags.StartAnchorWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuote);
        }

        public async void StartDivWithClass(string className)
        {
            await response.WriteAsync(HTMLTags.StartDivWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }

        public async void StartDivWithID(string id)
        {
            await response.WriteAsync(HTMLTags.StartDivWithID);
            await response.WriteAsync(id);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }

        public async void StartFigure(string className)
        {
            await response.WriteAsync(HTMLTags.StartFigureWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }

        public async void StartIMG(string path)
        {
            await response.WriteAsync(HTMLTags.StartImg);
            await response.WriteAsync(path);
            await response.WriteAsync(HTMLTags.CloseQuote);
        }

        public async void StartSpan(string className)
        {
            await response.WriteAsync(HTMLTags.StartSpanWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }
    }
}