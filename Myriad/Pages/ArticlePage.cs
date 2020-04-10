using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Feliciana.Data;
using Myriad.Data;
using Myriad.Library;
using Myriad.Parser;

namespace Myriad.Pages
{

    public struct ArticleHTML
    {
        public const string ArticleScripts = @"{
<script>
   window.onload = function () {
    AddShortcut();
    SetupIndex();
    SetupPartialPageLoad();
    HandleHiddenDetails();
    SetupSuppressedParagraphs();
    ScrollToTarget();
    });
</script>";
    }
    public class ArticlePage : CommonPage
    {
        public const string pageURL = "/Article";
        public const string queryKeyTitle = "Title";
        public const string queryKeyID = "ID";
        PageParser parser;

        string title;
        int id = Result.error;

        //todo implement article page
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
            try
            {
                var paragraphs = GetPageParagraphs();
                parser = new PageParser(writer);
                await Parse(paragraphs);
                await AddPageTitleData(writer);
                await AddPageHistory(writer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public List<string> GetPageParagraphs()
        {
            var reader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadArticle), id);
            var results = reader.GetData<string>();
            reader.Close();
            return results;
        }
        public override void LoadQueryInfo(IQueryCollection query)
        {
            try
            {
                string idstring;
                if (query.ContainsKey(queryKeyID))
                {
                    idstring = query[queryKeyID];
                    id = Numbers.Convert(idstring);
                    var titleReader = new DataReaderProvider<int>(
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
                    id = idReader.GetDatum<int>();
                    idReader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public override bool IsValid()
        {
            return (title != null) && (id != Result.error);
        }

        public async Task Parse(List<string> paragraphs)
        {
            parser.SetParagraphInfo(ParagraphType.Article, id);
            parser.SetStartHTML(HTMLTags.StartParagraphWithClass + HTMLClasses.comment +
                HTMLTags.CloseQuoteEndTag);
            parser.SetEndHTML(HTMLTags.EndParagraph);
            for (int i = Ordinals.first; i < paragraphs.Count; i++)
            {
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
            return HTMLTags.StartQuery + queryKeyID + '=' + id;
        }
    }
}
