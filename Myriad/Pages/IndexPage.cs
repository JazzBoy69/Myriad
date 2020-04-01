using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Parser;
using Myriad.Data;


namespace Myriad.Pages
{
    public struct IndexHTML
    {
        public const string IndexScripts = @"
<script>
   window.onload = function () {
    shortcut.add('Ctrl+F10', function () {
         document.getElementById('searchField').focus();
    });
    SetupIndex();
};
    </script>";
    }

    public class IndexPage : CommonPage
    {
        public const string pageURL = "/Index";

        int ID;
        public IndexPage() 
        {
        }

        protected override void RenderBody()
        {
            //todo move toc
            //todo edit page
            ID = GetPageID();
            var paragraphs = GetPageParagraphs();
            var parser = new NavigationParser(new HTMLResponseWriter(response));
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            parser.SetParagraphInfo(ParagraphType.Navigation, ID);
            parser.Parse(paragraphs);
        }

        private int GetPageID()
        {
            var reader = SQLServerReaderProvider<string>.Reader(DataOperation.ReadNavigationID, "home");
            return reader.GetDatum<int>();
        }

        public List<string> GetPageParagraphs()
        {
            var reader = SQLServerReaderProvider<string>.Reader(DataOperation.ReadNavigationPage, "home");
            return reader.GetData<string>();
        }

        protected override string GetTitle()
        {
            return "Myriad Study Bible";
        }

        protected override string PageScripts()
        {
            return IndexHTML.IndexScripts;
        }

        public override string GetURL()
        {
            return pageURL;
        }

        public override void LoadQueryInfo(IQueryCollection query)
        {
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
