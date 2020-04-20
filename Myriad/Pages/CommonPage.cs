using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
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

        public abstract Task LoadQueryInfo(IQueryCollection query);
        public abstract string GetQueryInfo();

        public abstract bool IsValid();
        public abstract string GetURL();
        async public Task RenderPage()
        {
            var writer = Writer.New(response);
            await WriteHeader(writer);
            await RenderBody(writer);
            await writer.Append(LayoutHTML.close);
            await writer.Append(LayoutHTML.tocdiv);
            await writer.Append(LayoutHTML.modalOverlay);
            await writer.Append(LayoutHTML.myriadJavaScript);
            await AddPageScripts();
            await writer.Append(LayoutHTML.endofBody);
        }

        protected async Task AddPageHistory(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartDivWithID+
                HTMLClasses.pageUrl+
                HTMLTags.CloseQuote+
                HTMLTags.Class+
                HTMLClasses.hidden+
                HTMLTags.CloseQuoteEndTag);
            await writer.Append(GetURL() + GetQueryInfo());
            await writer.Append(HTMLTags.EndDiv);
        }

        public async Task WriteHeader(HTMLWriter writer)
        {
            await writer.Append(LayoutHTML.startOfPage);
            await writer.Append(HTMLTags.StartTitle);
            await WriteTitle(writer);
            await writer.Append(HTMLTags.EndTitle);
            await writer.Append(LayoutHTML.header);
        }
        protected async Task AddPageTitleData(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartDivWithID+
                HTMLClasses.title+
                HTMLTags.CloseQuote+
                HTMLTags.Class+
                HTMLClasses.hidden+
                HTMLTags.CloseQuoteEndTag);
            await WriteTitle(writer);
            await writer.Append(HTMLTags.EndDiv);
        }

        protected abstract Task WriteTitle(HTMLWriter writer);

        public abstract Task RenderBody(HTMLWriter writer);

        async protected Task AddPageScripts()
        {
            await response.WriteAsync(PageScripts());
        }

        protected abstract string PageScripts();

        public abstract Task AddTOC(HTMLWriter writer);

        public abstract Task LoadTOCInfo(HttpContext context);
    }
}
