using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Parser;
using Myriad.Data;


namespace Myriad
{
    public struct IndexHTML
    {
        public const string IndexScripts = @"
<script>
   $(document).ready(function () {
    shortcut.add('Ctrl+F10', function () {
         $('#searchField').focus();
    });
    SetupPagination();
    SetupIndex();
});
    </script>";
    }

    public class IndexPage : CommonPage
    {
        public IndexPage(HttpResponse response) : base(response)
        {
            this.response = response;
        }

        async protected override Task RenderBody()
        {
            var paragraphs = GetPageParagraphs();
            var parser = new MarkupParser(new HTMLResponseWriter(response));
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            await parser.Parse(paragraphs);
        }

        public List<string> GetPageParagraphs()
        {
            return ReaderProvider.Reader()
                .GetData<string>(DataOperation.ReadNavigationPage, "home");
        }

        protected override string GetTitle()
        {
            return "Myriad Study Bible";
        }

        protected override string PageScripts()
        {
            return IndexHTML.IndexScripts;
        }
    }
}
