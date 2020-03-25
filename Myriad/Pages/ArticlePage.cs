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
            return ReaderProvider.Reader()
                .GetData<string>(DataOperation.ReadArticle, id);
        }
        public override void LoadQueryInfo(IQueryCollection query)
        {
            if (query.ContainsKey(queryKeyID))
            {
                title = ReaderProvider.Reader().GetDatum<string>(
                    DataOperation.ReadArticleTitle, id);
                return;
            }
            if (query.ContainsKey(queryKeyTitle))
            {
                title = query[queryKeyTitle];
                id = ReaderProvider.Reader().GetDatum<string>(
                        DataOperation.ReadArticleID, title);
            }
        }

        public override bool IsValid()
        {
            return (title != null) && (int.TryParse(id, out int result));
        }
    }
}
