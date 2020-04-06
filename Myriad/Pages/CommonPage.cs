using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Myriad.Parser;
using Myriad.Library;
using Myriad.Writer;

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
            var writer = WriterReference.New(response);
            await WriteHeader(writer);
            await RenderBody(writer);
            await AddPageTitleData(writer);
            await writer.Append(LayoutHTML.close);
            await writer.Append(LayoutHTML.tocdiv);
            await writer.Append(LayoutHTML.modalOverlay);
            await writer.Append(LayoutHTML.myriadJavaScript);
            await AddPageScripts();
            await writer.Append(LayoutHTML.endofBody);
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
            await writer.Append(HTMLTags.StartDivWithID);
            await writer.Append(HTMLClasses.title);
            await writer.Append(HTMLTags.CloseQuote);
            await writer.Append(HTMLTags.Class);
            await writer.Append(HTMLClasses.hidden);
            await writer.Append(HTMLTags.CloseQuoteEndTag);
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

        public abstract Task LoadTOCInfo();
    }
}
