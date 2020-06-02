using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Feliciana.Data;
using Myriad.Parser;
using Myriad.Data;
using Myriad.Library;


namespace Myriad.Pages
{
    public struct IndexHTML
    {
        public const string IndexScripts = @"
<script>
   window.onload = function () {
    AddShortcut();
    SetupIndex();
SetupPartialPageLoad();
ScrollToTop();
};
    </script>";
    }

    public class IndexPage : PaginationPage
    {
        public const string pageURL = "/Index";
        public const string editURL = "/Edit/Index";
        public const string queryKeyName = "name=";
        private const string requestNameQuery = "name";
        private const string defaultName = "home";
        List<string> paragraphs;
        int mainHeadingIndex;
        PageParser parser;
        string name = defaultName;
        int ID;
        public IndexPage() 
        {
            
        }
        public override async Task LoadQueryInfo(IQueryCollection query)
        {
            name = await Task.Run(() =>
            {
                if (query.ContainsKey(requestNameQuery))
                    return query[requestNameQuery].ToString();
                return defaultName;
            });
        }

        internal void SetName(string pageName)
        {
            name = pageName;
        }

        public async override Task RenderBody(HTMLWriter writer)
        {
            //todo edit page
            if (string.IsNullOrEmpty(name)) name = defaultName;
            ID = await GetPageID();
            paragraphs = GetPageParagraphs();
            parser = new PageParser(writer);
            parser.SetParagraphInfo(ParagraphType.Navigation, ID);
            await Parse();
            await AddPageTitleData(writer);
            await AddEditPageData(writer);
            await AddPageHistory(writer);
            await AddTOCButton(writer);
            if (name != defaultName) await AddPagination(writer);
        }
        private async Task AddEditPageData(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartDivWithID +
                HTMLClasses.editdata + HTMLTags.CloseQuote +
                HTMLTags.Class +
                HTMLClasses.hidden +
                HTMLTags.CloseQuoteEndTag +
                editURL + HTMLTags.StartQuery +
                queryKeyName);
            await writer.Append(name);
            await writer.Append(HTMLTags.EndDiv);
        }
        private async Task Parse()
        {
            bool foundFirstHeading = false;
            parser.SetStartHTML(HTMLTags.StartParagraphWithClass + HTMLClasses.comment + HTMLTags.CloseQuoteEndTag);
            parser.SetEndHTML(HTMLTags.EndParagraph);
            for (int index = Ordinals.first; index < paragraphs.Count; index++)
            {
                if (!foundFirstHeading)
                {
                    if ((paragraphs[index].Length > Number.nothing) &&
                        (paragraphs[index][Ordinals.first] == '='))
                    {
                        await parser.ParseMainHeading(paragraphs[index]);
                        foundFirstHeading = true;
                        mainHeadingIndex = index;
                    }
                    continue;
                }
                await parser.ParseParagraph(paragraphs[index], index);
            }
            await parser.EndComments();
        }

        private async Task<int> GetPageID()
        {
            var reader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadNavigationID), name);
            int result = await reader.GetDatum<int>();
            reader.Close();
            return result;
        }

        public List<string> GetPageParagraphs()
        {
            var reader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadNavigationPage), name);
            var results = reader.GetData<string>();
            reader.Close();
            return results;
        }

        internal override async Task WriteTitle(HTMLWriter writer)
        {
            var command = SqlServerInfo.GetCommand(DataOperation.ReadNavigationTitle);
            var reader = new DataReaderProvider<string>(command, name);
            string title = await reader.GetDatum<string>();
            await writer.Append(title);
            reader.Close();
            command.Connection.Close();
        }

        protected override string PageScripts()
        {
            return IndexHTML.IndexScripts;
        }

        public override string GetURL()
        {
            return pageURL;
        }

        public override bool IsValid()
        {
            return true;
        }

        public async override Task WriteTOC(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartList);
            await writer.Append(HTMLTags.ID);
            await writer.Append(HTMLClasses.toc);
            await writer.Append(HTMLTags.Class);
            await writer.Append(HTMLClasses.visible);
            await writer.Append(HTMLTags.CloseQuoteEndTag);
            var reader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadNavigationTitle), "");
            for (int index = Ordinals.first; index < mainHeadingIndex; index++)
            {
                reader.SetParameter(paragraphs[index]);
                string title = await reader.GetDatum<string>();
                if (string.IsNullOrEmpty(title)) title = paragraphs[index];
                await writer.Append(HTMLTags.StartListItem+
                    HTMLTags.EndTag+
                    HTMLTags.StartAnchor);
                Citation chapterCitation = Citation.InvalidCitation;
                await writer.Append(HTMLTags.HREF);
                if (title[Ordinals.first] == '#')
                {
                    chapterCitation = CitationConverter.FromString(title.Substring(Ordinals.second)).First();
                    await writer.Append(ChapterPage.pageURL+
                        HTMLTags.StartQuery+
                        ScripturePage.queryKeyStart+
                        Symbol.equal);
                    await writer.Append(chapterCitation.CitationRange.StartID.ID);
                    await writer.Append(HTMLTags.Ampersand+
                        ScripturePage.queryKeyEnd+
                        Symbol.equal);
                    await writer.Append(chapterCitation.CitationRange.EndID.ID);
                }
                else
                {
                    await writer.Append(pageURL+
                        HTMLTags.StartQuery+
                        queryKeyName);
                    await writer.Append(paragraphs[index]);
                }
                await writer.Append(HTMLTags.Ampersand+
                    HTMLClasses.partial+
                    HTMLTags.OnClick+
                    JavaScriptFunctions.HandleTOCClick+
                    HTMLTags.EndTag);
                if (title[Ordinals.first] == '#')
                {
                   await CitationConverter.ToLongString(chapterCitation, writer);
                }
                else
                {
                    await writer.Append(title);
                }
                await writer.Append(HTMLTags.EndAnchor+
                    HTMLTags.EndListItem);
            }
            reader.Close();
            await writer.Append(HTMLTags.EndList);
        }

        public override string GetQueryInfo()
        {
            return HTMLTags.StartQuery + queryKeyName + name;
        }

        public override Task LoadTOCInfo(HttpContext context)
        {
            paragraphs = GetPageParagraphs();
            for (int index = Ordinals.first; index < paragraphs.Count; index++)
            {
                if ((paragraphs[index].Length > Number.nothing) &&
                    (paragraphs[index][Ordinals.first] == '='))
                {
                    mainHeadingIndex = index;
                    break;
                }
            }
            return Task.CompletedTask;
        }

        public override async Task SetupNextPage()
        {
            var reader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadNextNavigationName),
                name);
            string newName = await reader.GetDatum<string>();
            name = ((string.IsNullOrEmpty(newName)) || (newName.Contains("=="))) ?
                name :
                newName;
            reader.Close();
        }

        public override async Task SetupPrecedingPage()
        {
            var reader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadPrecedingNavigationName),
                name);
            string newName = await reader.GetDatum<string>();
            name = (string.IsNullOrEmpty(newName)) ?
                name :
                newName;
            reader.Close();
        }

        public override async Task SetupParentPage()
        {
            var reader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadParentNavigationName),
                name);
            string newName = await reader.GetDatum<string>();
            name = (string.IsNullOrEmpty(newName)) ?
                name :
                newName;
            reader.Close();
        }

        public override async Task HandleEditRequest(HttpContext context)
        {
            await WritePlainText(Writer.New(context.Response),
                context.Request.Query);
        }

        private async Task WritePlainText(HTMLWriter writer, IQueryCollection query)
        {
            name = query[requestNameQuery];
            var paragraphs = GetPageParagraphs();
            for (int i = Ordinals.first; i < paragraphs.Count; i++)
            {
                await writer.Append(paragraphs[i]);
                await writer.Append(Symbol.lineFeed);
            }
        }

        public override async Task HandleAcceptedEdit(HttpContext context)
        {
            context.Request.Form.TryGetValue("text", out var text);
            name = context.Request.Query[requestNameQuery];
            var existingParagraphs = GetPageParagraphs();
            string textString = text.ToString().Trim();
            int id = await GetPageID();
            var newParagraphs = textString.Split(Symbols.linefeedArray, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (int i = Ordinals.first; i < newParagraphs.Count; i++)
            {
                if (i >= existingParagraphs.Count)
                {
                    await AddParagraph(newParagraphs[i], i, id);
                    continue;
                }
                if (existingParagraphs[i] == newParagraphs[i]) continue;
                await UpdateParagraph(newParagraphs[i], i, id);
            }
            for (int i = newParagraphs.Count; i < existingParagraphs.Count; i++)
            {
                await DeleteParagraph(i);
            }
            await RenderBody(Writer.New(context.Response));
        }

        private async Task AddParagraph(string paragraph, int index, int id)
        {
            await DataWriterProvider.Write<string, int, string, int>(
                SqlServerInfo.GetCommand(DataOperation.CreateNavigationParagraph),
                name, index, paragraph, id);
        }

        private async Task UpdateParagraph(string paragraph, int index, int id)
        {
            await DataWriterProvider.Write<int, int, string>(
                SqlServerInfo.GetCommand(DataOperation.UpdateNavigationParagraph),
                id, index, paragraph);
        }

        private async Task DeleteParagraph(int index)
        {
            await DataWriterProvider.Write<string, int>(
                SqlServerInfo.GetCommand(DataOperation.DeleteNavigationParagraph),
                name, index);
        }
    }
}
