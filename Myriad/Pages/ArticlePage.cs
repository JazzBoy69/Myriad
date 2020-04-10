using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Feliciana.Data;
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

        protected override async Task WriteTitle(HTMLWriter writer)
        {
            await writer.Append(title);
        }

        protected override string PageScripts()
        {
            return ArticleHTML.ArticleScripts;
        }

        public async override Task RenderBody(HTMLWriter writer)
        {
            var paragraphs = GetPageParagraphs();
            parser = new PageParser(writer);
            await Parse(paragraphs);
            await AddPageTitleData(writer);
        }
        public List<string> GetPageParagraphs()
        {
            var reader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadArticle), id);
            var results = reader.GetData<string>();
            reader.Close();
            return results;
        }
        public override void LoadQueryInfo(IQueryCollection query)
        {
            if (query.ContainsKey(queryKeyID))
            {
                id = query[queryKeyID];
                var titleReader = new DataReaderProvider<string>(
                    SqlServerInfo.GetCommand(DataOperation.ReadArticleTitle), id);
                title = titleReader.GetDatum<string>();
                titleReader.Close();
                return;
            }
            if (query.ContainsKey(queryKeyTitle))
            {
                title = query[queryKeyTitle];
                var idReader = new DataReaderProvider<string>(
                    SqlServerInfo.GetCommand(DataOperation.ReadArticleID), title);
                id = idReader.GetDatum<string>();
                idReader.Close();
            }
            idNumber = Numbers.Convert(id);
        }

        public override bool IsValid()
        {
            return (title != null) && (int.TryParse(id, out int result));
        }

        public async Task Parse(List<string> paragraphs)
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
                        await parser.ParseMainHeading(paragraphs[i]);

                        foundFirstHeading = true;
                    }
                    continue;
                }
                await parser.ParseParagraph(paragraphs[i], i);
            }
            await parser.EndComments();
        }

        public override Task AddTOC(HTMLWriter writer)
        {
            throw new NotImplementedException();
        }

        public override void LoadTOCInfo()
        {
            throw new NotImplementedException();
        }

        public override string GetQueryInfo()
        {
            return HTMLTags.StartQuery + queryKeyID + id;
        }
    }
}
