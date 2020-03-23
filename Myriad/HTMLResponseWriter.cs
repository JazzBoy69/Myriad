using Myriad.Parser;
using Microsoft.AspNetCore.Http;
namespace Myriad
{
    public class HTMLResponseWriter : HTMLResponse
    {
        private HttpResponse response;

        public HTMLResponseWriter(HttpResponse response)
        {
            this.response = response;
        }
        async void HTMLResponse.Append(char c)
        {
            await response.WriteAsync(c.ToString());
        }

        async void HTMLResponse.Append(string stringToAppend)
        {
            await response.WriteAsync(stringToAppend);
        }

        async void HTMLResponse.Append(int number)
        {
            await response.WriteAsync(number.ToString());
        }

        async void HTMLResponse.AppendClass(string className)
        {
            await response.WriteAsync(HTMLTags.Class);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuote);
        }

        async void HTMLResponse.AppendHREF(string pageName)
        {
            await response.WriteAsync(HTMLTags.HREF);
            await response.WriteAsync(pageName);
        }

        async void HTMLResponse.AppendIMGWidth(string widthString)
        {
            await response.WriteAsync(HTMLTags.Width);
            await response.WriteAsync(widthString);
            await response.WriteAsync(HTMLTags.CloseQuote);
        }

        string HTMLResponse.Response()
        {
            return "";
        }

        async void HTMLResponse.StartAnchor(string className)
        {
            await response.WriteAsync(HTMLTags.StartAnchorWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuote);
        }

        async void HTMLResponse.StartDivWithClass(string className)
        {
            await response.WriteAsync(HTMLTags.StartDivWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }

        async void HTMLResponse.StartDivWithID(string id)
        {
            await response.WriteAsync(HTMLTags.StartDivWithID);
            await response.WriteAsync(id);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }

        async void HTMLResponse.StartFigure(string className)
        {
            await response.WriteAsync(HTMLTags.StartFigureWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }

        async void HTMLResponse.StartIMG(string path)
        {
            await response.WriteAsync(HTMLTags.StartImg);
            await response.WriteAsync(path);
            await response.WriteAsync(HTMLTags.CloseQuote);
        }

        async void HTMLResponse.StartSpan(string className)
        {
            await response.WriteAsync(HTMLTags.StartSpanWithClass);
            await response.WriteAsync(className);
            await response.WriteAsync(HTMLTags.CloseQuoteEndTag);
        }
    }
}