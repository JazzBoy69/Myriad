using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Feliciana.ResponseWriter;

namespace Myriad.Pages
{
    public class SearchPage : CommonPage
    {
        public const string pageURL = "/Search";
        //todo implement search page
        public override string GetURL()
        {
            throw new NotImplementedException();
        }

        public override bool IsValid()
        {
            throw new NotImplementedException();
        }

        public override Task LoadQueryInfo(IQueryCollection query)
        {
            throw new NotImplementedException();
        }

        protected override async Task WriteTitle(HTMLWriter writer)
        {
            throw new NotImplementedException();
        }

        protected override string PageScripts()
        {
            throw new NotImplementedException();
        }

        public async override Task RenderBody(HTMLWriter writer)
        {
            throw new NotImplementedException();
            await AddPageTitleData(writer);
        }

        public override Task AddTOC(HTMLWriter writer)
        {
            throw new NotImplementedException();
        }

        public override Task LoadTOCInfo(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public override string GetQueryInfo()
        {
            throw new NotImplementedException();
        }
    }
}
