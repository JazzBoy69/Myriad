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
</script>";
    }
    public class ArticlePage : CommonPage
    {
        public const string pageURL = "/Article";
        public const string queryKeyTitle = "Title";
        public const string queryKeyID = "ID";
        PageParser parser;
        List<string> headings;

        string title;
        int id = Result.error;
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
            await AddMainHeading(writer);
            await Parse(paragraphs);
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
        }

        private async Task AddMainHeading(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartMainHeader);
            await WriteTitle(writer);
            await writer.Append(HTMLTags.EndMainHeader);
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
            string idstring;
            if ((query.ContainsKey(queryKeyID)) && (query.ContainsKey(queryKeyTitle)))
            {
                idstring = query[queryKeyID];
                id = Numbers.Convert(idstring);
                title = query[queryKeyTitle];
                return;
            }
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

        public override async Task AddTOC(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartList);
            await writer.Append(HTMLTags.ID);
            await writer.Append(HTMLClasses.toc);
            await writer.Append(HTMLTags.Class);
            await writer.Append(HTMLClasses.visible);
            await writer.Append(HTMLTags.CloseQuoteEndTag);
            for (int index = Ordinals.first; index < headings.Count; index++)
            {
                await writer.Append(HTMLTags.StartListItem);
                await writer.Append(HTMLTags.EndTag);
                await writer.Append(HTMLTags.StartAnchor);
                await writer.Append(HTMLTags.HREF);
                await writer.Append("#header");
                await writer.Append(index);
                await writer.Append(HTMLTags.OnClick);
                await writer.Append(JavaScriptFunctions.HandleTOCClick);
                await writer.Append(HTMLTags.EndTag);
                await writer.Append(headings[index]);
                await writer.Append(HTMLTags.EndAnchor);
                await writer.Append(HTMLTags.EndListItem);
            }
            await writer.Append(HTMLTags.EndList);
        }

        public override void LoadTOCInfo()
        {
            var paragraphs = GetPageParagraphs();
            headings = new List<string>();
            for (int index = Ordinals.first; index < paragraphs.Count; index++)
            {
                if ((paragraphs[index].Length > Number.nothing) &&
                    (paragraphs[index][Ordinals.first] == '='))
                {
                    headings.Add(paragraphs[index].Replace("==", ""));
                }
            }
        }

        public override string GetQueryInfo()
        {
            return HTMLTags.StartQuery + queryKeyID + '=' + id;
        }
    }
}
