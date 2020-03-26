using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;
using Myriad.Data;
using Myriad.Parser;

namespace Myriad.Pages
{

    public struct ArticleHTML
    {
        public const string ArticleScripts = @"{
<script>
   window.onload = function () {
    shortcut.add('Ctrl+F10', function () {
         document.getElementById('searchField').focus();
    });
    CreateTableOfContents('#comments');
    SetupIndex();
    HandleHiddenDetails();
	SetupOverlay();
    SetupModalPictures();
    SetupSuppressedParagraphs();
    SetupEditParagraph();
    ScrollToTarget();
    });
</script>";
    }
    public class ArticlePage : CommonPage
    {
        public const string pageURL = "/Article";
        public const string queryKeyTitle = "Title=";
        public const string queryKeyID = "ID=";

        string title;
        string id;
        public ArticlePage()
        {
        }

        public override string GetURL()
        {
            return pageURL;
        }

        protected override string GetTitle()
        {
            return title;
        }

        protected override string PageScripts()
        {
            return ArticleHTML.ArticleScripts;
        }

        protected override void RenderBody()
        {
            var paragraphs = GetPageParagraphs();
            var parser = new ArticleParser(new HTMLResponseWriter(response));
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            parser.Parse(paragraphs);
        }
        public List<string> GetPageParagraphs()
        {
            var reader = ReaderProvider<string>.Reader(DataOperation.ReadArticle, id);
            return reader.GetData<string>();
        }
        public override void LoadQueryInfo(IQueryCollection query)
        {
            if (query.ContainsKey(queryKeyID))
            {
                id = query[queryKeyID];
                var titleReader = ReaderProvider<string>.Reader(
                    DataOperation.ReadArticleTitle, id);
                title = titleReader.GetDatum<string>();
                return;
            }
            if (query.ContainsKey(queryKeyTitle))
            {
                title = query[queryKeyTitle];
                var idReader = ReaderProvider<string>.Reader(
                    DataOperation.ReadArticleID, title);
                id = idReader.GetDatum<string>();
            }
        }

        public override bool IsValid()
        {
            return (title != null) && (int.TryParse(id, out int result));
        }
    }
}
