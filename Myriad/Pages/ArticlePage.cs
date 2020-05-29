using System;
using System.Collections.Generic;
using System.Linq;
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
};
</script>";
    }
    public class ArticlePage : CommonPage
    {
        public const string pageURL = "/Article";
        public const string addArticleURL = "/AddArticle";
        public const string editURL = "/Edit/Article";
        public const string queryKeyTitle = "Title";
        public const string queryKeyID = "ID";
        PageParser parser;
        List<string> headings;
        Citation targetCitation;

        (string Title, int ID) pageInfo = ("", Result.error);
        public ArticlePage()
        {
        }


        public override string GetURL()
        {
            return pageURL;
        }

        internal override async Task WriteTitle(HTMLWriter writer)
        {
            await writer.Append(pageInfo.Title);
        }

        protected override string PageScripts()
        {
            return ArticleHTML.ArticleScripts;
        }

        public async override Task RenderBody(HTMLWriter writer)
        {
            var paragraphs = GetPageParagraphs();
            parser = new PageParser(writer);
            if ((targetCitation != null) && (targetCitation.CitationRange.Valid))
            {
                parser.SetTargetRange(targetCitation.CitationRange);
            }
            await AddMainHeading(writer);
            var paragraphIndices = ReadParagraphIndices();
            if (paragraphIndices.Count > 1)
                await Parse(paragraphs, paragraphIndices);
            else
                await Parse(paragraphs);
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
            await AddEditPageData(writer);
            await AddTOCButton(writer);
        }

        private List<int> ReadParagraphIndices()
        {
            if (targetCitation == null) return new List<int>();
            var reader = new DataReaderProvider<int, int, int>(
                SqlServerInfo.GetCommand(DataOperation.ParagraphsThatContainVerse),
                pageInfo.ID, targetCitation.CitationRange.StartID.ID, targetCitation.CitationRange.EndID.ID);
            var result = reader.GetData<int>();
            reader.Close();
            return result;
        }

        internal async Task AddArticle(HTMLWriter writer, IQueryCollection query)
        {
            (string title, int similarID) = await GetID(query);
            int id = await GetNewArticleID();
            title = query[queryKeyTitle];
            pageInfo = (title, id);
            await AddSynonym(id, Ordinals.first, title);
            if (similarID > Number.nothing) title += " (Already Exists)";
            await DataWriterProvider.Write(SqlServerInfo.GetCommand(DataOperation.CreateTag),
                id, title);
            await RenderBody(writer);
        }

        private async Task<int> GetNewArticleID()
        {
            var reader = new DataReaderProvider(SqlServerInfo.GetCommand(DataOperation.ReadMaxArticleID));
            int id = await reader.GetDatum<int>();
            reader.Close();
            return id + 1;
        }

        internal async Task UpdateArticle(HTMLWriter writer, IQueryCollection query, string text)
        {
            (string title, int id) = await GetTitle(query);
            var paragraphs = GetPageParagraphs(id);
            var lines = text.Split(Symbols.linefeedArray, StringSplitOptions.RemoveEmptyEntries);
            await UpdateArticleTitle(id, title, lines[Ordinals.first].Replace("==", ""));
            await UpdateSynonyms(id, lines[Ordinals.second]);
            var newParagraphs = lines[Ordinals.third..].ToList();
            parser = new PageParser(writer);
            pageInfo.ID = id;
            pageInfo.Title = await Reader.ReadTitle(id);
            await AddMainHeading(writer);
            parser.SetParagraphInfo(ParagraphType.Article, pageInfo.ID);
            parser.SetStartHTML(HTMLTags.StartParagraphWithClass + HTMLClasses.comment +
                HTMLTags.CloseQuoteEndTag);
            parser.SetEndHTML(HTMLTags.EndParagraph);
            await DeleteDefinitionSearches(id);
            for (int i = Ordinals.first; i < newParagraphs.Count; i++)
            {
                ArticleParagraph articleParagraph = new ArticleParagraph(id, i, newParagraphs[i]);
                if ((i < paragraphs.Count) && (newParagraphs[i] != paragraphs[i]))
                {
                    await EditParagraph.UpdateArticleParagraph(parser, articleParagraph);
                    continue;
                }
                if (i >= paragraphs.Count)
                {
                    await EditParagraph.AddArticleParagraph(parser, articleParagraph);
                    continue;
                }
                await parser.ParseParagraph(paragraphs[i], i);
                await EditParagraph.AddDefinitionSearches(articleParagraph, parser.Citations);
            }
            await parser.EndComments();
            if (newParagraphs.Count < paragraphs.Count)
            {
                for (int i = newParagraphs.Count; i < paragraphs.Count; i++)
                {
                    await EditParagraph.DeleteArticleParagraph(pageInfo.ID, i);
                }
            }
            await AddEditPageData(writer);
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
        }

        private async Task DeleteDefinitionSearches(int id)
        {
            await DataWriterProvider.Write(SqlServerInfo.GetCommand(DataOperation.DeleteDefinitionSearches), id);
        }

        private async Task UpdateSynonyms(int id, string newSynonymLine)
        {
            var oldSynonyms = GetSynonyms(id);
            var newSynonyms = newSynonymLine.Split(Symbols.spaceArray, StringSplitOptions.RemoveEmptyEntries).ToList();
            int identifierIndex = GetIdentifierIndex(newSynonyms);
            if (identifierIndex != Result.notfound)
            {
                await UpdateIdentifier(id, newSynonyms[identifierIndex].Substring(Ordinals.second));
                newSynonyms.RemoveAt(identifierIndex);
            }
            for (int i = Ordinals.first; i < newSynonyms.Count; i++)
            {
                if (i >= oldSynonyms.Count)
                {
                    await AddSynonym(id, i, newSynonyms[i].Replace('_', ' '));
                    continue;
                }
                await UpdateSynonym(id, i, newSynonyms[i].Replace('_', ' '));
            }
            if (oldSynonyms.Count > newSynonyms.Count)
            {
                await DeleteSynonyms(id, newSynonyms.Count);
            }

        }

        private async Task UpdateIdentifier(int id, string identifier)
        {
            string oldIdentifier = await GetIdentifier(id);
            if (string.IsNullOrEmpty(oldIdentifier))
            {
                await DataWriterProvider.Write<int, string>(
                    SqlServerInfo.GetCommand(DataOperation.CreateIdentifier), id, identifier);
                return;
            }
            if (oldIdentifier == identifier) return;
            await DataWriterProvider.Write<int, string>(
                SqlServerInfo.GetCommand(DataOperation.UpdateIdentifier), id, identifier);
        }

        private int GetIdentifierIndex(List<string> synonyms)
        {
            for (int i = Ordinals.first; i < synonyms.Count; i++)
            {
                if (synonyms[i][Ordinals.first] == '+') return i;
            }
            return Result.notfound;
        }

        private async Task DeleteSynonyms(int id, int startIndex)
        {
            await DataWriterProvider.Write(SqlServerInfo.GetCommand(DataOperation.DeleteSynonyms),
                id, startIndex);
        }

        private async Task UpdateSynonym(int id, int index, string synonym)
        {
            if (synonym.Contains(' ')) await Phrases.Add(synonym);
            await DataWriterProvider.Write(SqlServerInfo.GetCommand(DataOperation.UpdateSynonym),
                id, index, synonym);
        }

        private async Task AddSynonym(int id, int index, string synonym)
        {
            if (synonym.Contains(' ')) await Phrases.Add(synonym);
            await DataWriterProvider.Write(SqlServerInfo.GetCommand(DataOperation.CreateSynonym),
                id, index, synonym);
        }

        private async Task UpdateArticleTitle(int id, string originalTitle, string newTitle)
        {
            if (originalTitle == newTitle) return;
            await DataWriterProvider.Write(SqlServerInfo.GetCommand(DataOperation.UpdateArticleTitle), id, newTitle);
        }

        internal async Task WritePlainText(HTMLWriter writer, IQueryCollection query)
        {
            (string title, int id) = await GetTitle(query);
            await writer.Append("==");
            await writer.Append(title);
            await writer.Append("==" + Symbol.lineFeed);
            var articleIdentifier = await GetIdentifier(id);
            if (!string.IsNullOrEmpty(articleIdentifier))
            {
                await writer.Append("+");
                await writer.Append(articleIdentifier);
                await writer.Append(" ");
            }
            var synonyms = GetSynonyms(id);
            for (int i = Ordinals.first; i < synonyms.Count; i++)
            {
                if (i > Ordinals.first) await writer.Append(" ");
                await writer.Append(synonyms[i].Replace(' ', '_'));
            }
            await writer.Append(Symbol.lineFeed);
            var paragraphs = GetPageParagraphs(id);
            for (int i = Ordinals.first; i < paragraphs.Count; i++)
            {
                await writer.Append(paragraphs[i]);
                await writer.Append(Symbol.lineFeed);
            }
            await EditParagraph.CheckDefinitionSearchesForParagraphIndices(id);
        }

        private async Task<string> GetIdentifier(int id)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadArticleIdentifier),
                id);
            string identifier = await reader.GetDatum<string>();
            reader.Close();
            return identifier;
        }

        private async Task AddEditPageData(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartDivWithID +
                HTMLClasses.editdata + HTMLTags.CloseQuote+
                HTMLTags.Class+
                HTMLClasses.hidden+
                HTMLTags.CloseQuoteEndTag+
                editURL+HTMLTags.StartQuery+
                queryKeyID+Symbol.equal);
            await writer.Append(pageInfo.ID);
            await writer.Append(HTMLTags.EndDiv);
        }

        private async Task AddMainHeading(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartMainHeader);
            await WriteTitle(writer);
            await writer.Append(HTMLTags.EndMainHeader);
        }

        public List<string> GetPageParagraphs()
        {
            return GetPageParagraphs(pageInfo.ID);
        }

        public static List<string> GetPageParagraphs(int id)
        {
            var reader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadArticle), id);
            var results = reader.GetData<string>();
            reader.Close();
            return results;
        }
        public override async Task LoadQueryInfo(IQueryCollection query)
        {
            if (query.ContainsKey(ScripturePage.queryKeyTGStart))
            {
                targetCitation = new Citation(Numbers.Convert(query[ScripturePage.queryKeyTGStart].ToString()),
                    Numbers.Convert(query[ScripturePage.queryKeyTGEnd].ToString()));
            }
            if ((query.ContainsKey(queryKeyID)) && (query.ContainsKey(queryKeyTitle)))
            {
                pageInfo = await GetInfoFromQuery(query);
                return;
            }
            if (query.ContainsKey(queryKeyID))
            {
                pageInfo = await GetTitle(query);
                return;
            }
            if (query.ContainsKey(queryKeyTitle))
            {
                pageInfo = await GetID(query);
            }
        }

        private async Task<(string Title, int ID)> GetInfoFromQuery(IQueryCollection query)
        {
            (string Title, int ID) result =
            await Task.Run(() =>
            {
                string idString = query[queryKeyID];
                int id = Numbers.Convert(idString);
                string title = query[queryKeyTitle];
                return result = (title, id);
            });
            return result;
        }

        private async Task<(string title, int id)> GetID(IQueryCollection query)
        {
            string queryTitle = query[queryKeyTitle];
            int id = await GetIdFromIdentifier(queryTitle);
            if (id > 0)
            {
                return (await Reader.ReadTitle(id), id);
            }
            string title = Inflections.RootsOf(queryTitle.Replace('_', ' ')).First();
            var idReader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadArticleID), title);
            id = await idReader.GetDatum<int>();
            idReader.Close();
            if (id>0) return (title, id);
            var synonymID = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadIDFromSynonym), title);
            id = await synonymID.GetDatum<int>();
            synonymID.Close();
            title = await Reader.ReadTitle(id);
            return (title, id);
        }

        private async Task<int> GetIdFromIdentifier(string identifier)
        {
            var reader = new DataReaderProvider<string>(SqlServerInfo.GetCommand(DataOperation.ReadIDFromIdentifier),
                identifier);
            int id = await reader.GetDatum<int>();
            reader.Close();
            return id;
        }

        private async Task<(string title, int id)> GetTitle(IQueryCollection query)
        {
            string idstring = query[queryKeyID];
            int id = Numbers.Convert(idstring);
            string title = await Reader.ReadTitle(id);
            return (title, id);
        }

        public override bool IsValid()
        {
            return (pageInfo.Title != null) && (pageInfo.ID != Result.error);
        }

        public async Task Parse(List<string> paragraphs)
        {
            parser.SetParagraphInfo(ParagraphType.Article, pageInfo.ID);
            parser.SetStartHTML(HTMLTags.StartParagraphWithClass + HTMLClasses.comment +
                HTMLTags.CloseQuoteEndTag);
            parser.SetEndHTML(HTMLTags.EndParagraph);
            for (int i = Ordinals.first; i < paragraphs.Count; i++)
            {
                await parser.ParseParagraph(paragraphs[i], i);
            }
            await parser.EndComments();
        }

        public async Task Parse(List<string> paragraphs, List<int> paragraphIndices)
        {
            parser.SetParagraphInfo(ParagraphType.Article, pageInfo.ID);
            parser.SetEndHTML(HTMLTags.EndParagraph);
            for (int i = Ordinals.first; i < paragraphs.Count; i++)
            {
                if (paragraphIndices.Contains(i))
                {
                    parser.SetStartHTML(HTMLTags.StartParagraphWithClass + HTMLClasses.comment +
                        HTMLTags.CloseQuoteEndTag);
                }
                else
                {
                    parser.SetStartHTML(HTMLTags.StartParagraphWithClass + HTMLClasses.comment +
                        Symbol.space + HTMLClasses.suppressed + HTMLTags.CloseQuoteEndTag);
                }
                await parser.ParseParagraph(paragraphs[i], i);
            }
            await parser.EndComments();
        }

        public override async Task WriteTOC(HTMLWriter writer)
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

        public override async Task LoadTOCInfo(HttpContext context)
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
            return HTMLTags.StartQuery + queryKeyID + '=' + pageInfo.ID;
        }

        internal static List<string> GetSynonyms(int articleID)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadSynonymsFromID),
                articleID);
            List<string> result = reader.GetData<string>();
            reader.Close();
            return result;
        }

        public override async Task HandleEditRequest(HttpContext context)
        {
            await WritePlainText(Writer.New(context.Response),
                        context.Request.Query);
        }

        public override async Task HandleAcceptedEdit(HttpContext context)
        {
            context.Request.Form.TryGetValue("text", out var text);
            await UpdateArticle(Writer.New(context.Response),
                context.Request.Query, text.ToString());
        }
    }
}
