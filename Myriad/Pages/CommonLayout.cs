using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Myriad.Parser;

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
        public abstract string GetURL();
        async public Task RenderPage()
        {
            await CommonLayout.WriteHeader(response, GetTitle());
            RenderBody();
            await Write(LayoutHTML.close);
            AddPageScripts();
            await Write(LayoutHTML.endofBody);
        }

        protected abstract string GetTitle();

        async private Task Write(string stringToWrite)
        {
            await response.WriteAsync(stringToWrite);
        }

        protected abstract void RenderBody();

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
