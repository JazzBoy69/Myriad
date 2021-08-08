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
            var paragraphs = await DataRepository.Article(pageInfo.ID);
            parser = new PageParser(writer);
            if ((targetCitation != null) && (targetCitation.Valid))
            {
                parser.SetTargetRange(targetCitation.citationRange);
            }
            await AddMainHeading(writer);
            var paragraphIndices = await DataRepository.ParagraphsThatContainRange(pageInfo.ID,
                targetCitation.Start, targetCitation.End);
            if (paragraphIndices.Count > 1)
                await Parse(paragraphs, paragraphIndices);
            else
                await Parse(paragraphs);
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
            await AddEditPageData(writer);
            await AddTOCButton(writer);
        }

        internal async Task AddArticle(HTMLWriter writer, IQueryCollection query)
        {
            string title = query[queryKeyTitle].ToString();
            int similarID = await GetSimilarID(title);
            int id = await GetNewArticleID();
            title = query[queryKeyTitle];
            pageInfo = (title, id);
            if (similarID > Number.nothing) title += " (Already Exists)";
            await CreateNewArticle(title, id);
            await RenderBody(writer);
        }

        private async Task<int> GetNewArticleID()
        {
            int id = await DataRepository.MaxArticleID();
            return id + 1;
        }

        internal async Task UpdateArticle(HTMLWriter writer, IQueryCollection query, string text)
        {
            (string title, int id) = await GetTitle(query);
            var paragraphs = await DataRepository.Article(id);
            var lines = text.Split(Symbols.linefeedArray, StringSplitOptions.RemoveEmptyEntries);
            await UpdateArticleTitle(id, title, lines[Ordinals.first].Replace("==", ""));
            await UpdateSynonyms(id, lines[Ordinals.second]);
            var newParagraphs = lines[Ordinals.third..].ToList();
            parser = new PageParser(writer);
            pageInfo.ID = id;
            pageInfo.Title = await DataRepository.Title(id);
            await AddMainHeading(writer);
            parser.SetParagraphInfo(ParagraphType.Article, pageInfo.ID);
            parser.SetStartHTML(HTMLTags.StartParagraphWithClass + HTMLClasses.comment +
                HTMLTags.CloseQuoteEndTag);
            parser.SetEndHTML(HTMLTags.EndParagraph);
            await DataRepository.DeleteDefinitionSearches(id);
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
                if (paragraphs[i].Length > 1)
                {
                    string token = paragraphs[i].Substring(Ordinals.first, 2);
                    if ((token == "[|") || (token == "|-") || (token == "||")) parser.Citations.Clear();
                }
                await EditParagraph.AddDefinitionSearches(articleParagraph, parser.Citations);
            }
            await parser.EndComments();
            if (newParagraphs.Count < paragraphs.Count)
            {
                for (int i = newParagraphs.Count; i < paragraphs.Count; i++)
                {
                    await DataRepository.DeleteGlossaryParagraph(pageInfo.ID, i);
                }
            }
            await AddEditPageData(writer);
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
        }
        private async Task UpdateSynonyms(int id, string newSynonymLine)
        {
            var oldSynonyms = await DataRepository.Synonyms(id);
            var synonyms = newSynonymLine.Split(Symbols.spaceArray, StringSplitOptions.RemoveEmptyEntries).ToList();
            var newSynonyms = new List<string>();
            for (int i = Ordinals.first; i < synonyms.Count; i++)
            {
                if (synonyms[i].Contains('_') || synonyms[i].Contains('+'))
                {
                    newSynonyms.Add(synonyms[i].Replace('_', ' '));
                    continue;
                }
                newSynonyms.Add(Inflections.RootsOf(synonyms[i]).First());
            }
            int identifierIndex = GetIdentifierIndex(newSynonyms);
            if (identifierIndex != Result.notfound)
            {
                await UpdateIdentifier(id, newSynonyms[identifierIndex].Substring(Ordinals.second));
                newSynonyms.RemoveAt(identifierIndex);
            }
            await DataRepository.DeleteSynonyms(id);
            await DataRepository.WriteSynonyms(id, newSynonyms);
        }

        private async Task UpdateIdentifier(int id, string identifier)
        {
            string oldIdentifier = await DataRepository.DefinitionID(id);
            if (string.IsNullOrEmpty(oldIdentifier))
            {
                await DataRepository.WriteDefinitionID(id, identifier);
                return;
            }
            if (oldIdentifier == identifier) return;
            await DataRepository.UpdateDefinitionID(id, identifier);
        }

        private int GetIdentifierIndex(List<string> synonyms)
        {
            for (int i = Ordinals.first; i < synonyms.Count; i++)
            {
                if (synonyms[i][Ordinals.first] == '+') return i;
            }
            return Result.notfound;
        }

        private async Task AddSynonym(int id, string synonym)
        {
            if (synonym.Contains(' ')) await Phrases.Add(synonym);
            await DataRepository.WriteSynonyms(id, new List<string>() { synonym });
        }

        private async Task UpdateArticleTitle(int id, string originalTitle, string newTitle)
        {
            if (originalTitle == newTitle) return;
            await DataRepository.UpdateTag(id, newTitle);
        }

        internal async Task WritePlainText(HTMLWriter writer, IQueryCollection query)
        {
            (string title, int id) = await GetTitle(query);
            await writer.Append("==");
            await writer.Append(title);
            await writer.Append("==" + Symbol.lineFeed);
            string articleIdentifier = await DataRepository.DefinitionID(id);
            if (!string.IsNullOrEmpty(articleIdentifier))
            {
                await writer.Append("+");
                await writer.Append(articleIdentifier);
                await writer.Append(" ");
            }
            var synonyms = await DataRepository.Synonyms(id);
            for (int i = Ordinals.first; i < synonyms.Count; i++)
            {
                if (i > Ordinals.first) await writer.Append(" ");
                await writer.Append(synonyms[i].Replace(' ', '_'));
            }
            await writer.Append(Symbol.lineFeed);
            var paragraphs = await DataRepository.Article(id);
            for (int i = Ordinals.first; i < paragraphs.Count; i++)
            {
                await writer.Append(HTMLTags.StartParagraph);
                await writer.Append(paragraphs[i]);
                await writer.Append(HTMLTags.EndParagraph);
            }
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
        public void SetPageInfo(string title, int id)
        {
            pageInfo = (title, id);
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
            int id = await GetSimilarID(queryTitle);
            string title;
            if (id == 0)
            {
                id = await GetNewArticleID();
                title = query[queryKeyTitle];
                await CreateNewArticle(title, id);
                return (title.Replace('_', ' '), id);
            }
            title = await DataRepository.Title(id);
            return (title, id);
        }

        private async Task<int> GetSimilarID(string queryTitle)
        {
            int id = await DataRepository.ArticleIDFromIdentifier(queryTitle);
            if (id > 0)
            {
                return id;
            }
            queryTitle = queryTitle.Replace('_', ' ');
            string title = Inflections.RootsOf(queryTitle).First();
            id = await DataRepository.ArticleID(title);
            if (id > 0) return id;
            id = await DataRepository.ArticleIDFromSynonym(queryTitle);
            if (id > 0)
            {
                return id;
            }
            id = await DataRepository.ArticleIDFromSynonym(title);
            return id;
        }

        private async Task CreateNewArticle(string title, int id)
        {
            await AddSynonym(id, title);
            await DataRepository.WriteTag(id, title.Replace('_', ' '));
        }

        private async Task<(string title, int id)> GetTitle(IQueryCollection query)
        {
            if (query.ContainsKey(queryKeyTitle)) return await GetID(query);
            string idstring = query[queryKeyID];
            int id = Numbers.Convert(idstring);
            string title = await DataRepository.Title(id);
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
            var paragraphs = await DataRepository.Article(pageInfo.ID);
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
