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
        PageParser parser;

        string title;
        string id;
        int idNumber;
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

        public override void RenderBody(HTMLWriter writer)
        {
            var paragraphs = GetPageParagraphs();
            parser = new PageParser(writer);
            Parse(paragraphs);
        }
        public List<string> GetPageParagraphs()
        {
            var reader = SQLServerReaderProvider<string>.Reader(DataOperation.ReadArticle, id);
            return reader.GetData<string>();
        }
        public override void LoadQueryInfo(IQueryCollection query)
        {
            if (query.ContainsKey(queryKeyID))
            {
                id = query[queryKeyID];
                var titleReader = SQLServerReaderProvider<string>.Reader(
                    DataOperation.ReadArticleTitle, id);
                title = titleReader.GetDatum<string>();
                return;
            }
            if (query.ContainsKey(queryKeyTitle))
            {
                title = query[queryKeyTitle];
                var idReader = SQLServerReaderProvider<string>.Reader(
                    DataOperation.ReadArticleID, title);
                id = idReader.GetDatum<string>();

            }
            idNumber = Numbers.Convert(id);
        }

        public override bool IsValid()
        {
            return (title != null) && (int.TryParse(id, out int result));
        }

        public void Parse(List<string> paragraphs)
        {
            bool foundFirstHeading = false;
            parser.SetParagraphInfo(ParagraphType.Article, idNumber);
            for (int i = Ordinals.first; i < paragraphs.Count; i++)
            {
                if (!foundFirstHeading)
                {
                    if ((paragraphs[i].Length > Number.nothing) &&
                        (paragraphs[i][Ordinals.first] == '='))
                    {
                        parser.ParseMainHeading(paragraphs[i]);

                        foundFirstHeading = true;
                    }
                    continue;
                }
                parser.ParseParagraph(paragraphs[i], i);
            }
            parser.EndComments();
        }
    }
}
