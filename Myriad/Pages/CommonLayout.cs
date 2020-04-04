using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Myriad.Parser;
using Myriad.Library;

namespace Myriad.Pages
{
    public abstract class CommonPage
    {
        protected HttpResponse response;

        public CommonPage()
        {
        }

        public void SetResponse(HttpResponse response)
        {
            this.response = response;
        }

        public abstract void LoadQueryInfo(IQueryCollection query);

        public abstract bool IsValid();
        public abstract string GetURL();
        async public Task RenderPage()
        {
            await CommonLayout.WriteHeader(response, GetTitle());
            RenderBody(new HTMLResponseWriter(response));
            await AddPageTitleData();
            await Write(LayoutHTML.close);
            AddPageScripts();
            await Write(LayoutHTML.endofBody);
        }

        protected async Task AddPageTitleData()
        {
            await Write(HTMLTags.StartDivWithID);
            await Write(HTMLClasses.title);
            await Write(HTMLTags.CloseQuote);
            await Write(HTMLTags.Class);
            await Write(HTMLClasses.hidden);
            await Write(HTMLTags.CloseQuoteEndTag);
            await Write(GetTitle());
            await Write(HTMLTags.EndDiv);
        }

        protected abstract string GetTitle();

        async private Task Write(string stringToWrite)
        {
            await response.WriteAsync(stringToWrite);
        }

        public abstract void RenderBody(HTMLWriter writer);

        async protected void AddPageScripts()
        {
            await response.WriteAsync(PageScripts());
        }

        protected abstract string PageScripts();
    }

    public class CommonLayout
    {
        public static async Task WriteHeader(HttpResponse response, string title)
        {
            await response.WriteAsync(LayoutHTML.startOfPage);
            await response.WriteAsync("<title>");
            await response.WriteAsync(title);
            await response.WriteAsync("</title>");
            await response.WriteAsync(LayoutHTML.header);
        }
    }
}
