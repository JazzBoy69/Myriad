using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
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
HandleResize();
HandleGestures();
AddShortcut();
SetIcons();
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
                    return query[requestNameQuery].ToString().Replace("_", " ");
                return defaultName;
            });
        }

        internal void SetName(string pageName)
        {
            name = pageName.Replace("_", " ");
        }

        public async override Task RenderBody(HTMLWriter writer)
        {
            //todo edit page
            if (string.IsNullOrEmpty(name)) name = defaultName;
            ID = await GetPageID();
            paragraphs = await GetPageParagraphs();
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
            int id = await DataRepository.NavigationID(name);
            return id;
        }

        public async Task<List<string>> GetPageParagraphs()
        {
            var paragraphs = await DataRepository.NavigationParagraphs(ID);
            return paragraphs;
        }

        internal override async Task WriteTitle(HTMLWriter writer)
        {
            string title = await DataRepository.NavigationHeading(name);
            await writer.Append(title);
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
            for (int index = Ordinals.first; index < mainHeadingIndex; index++)
            {
                string title = await DataRepository.NavigationHeading(paragraphs[index]);
                if (string.IsNullOrEmpty(title)) title = paragraphs[index];
                await writer.Append(HTMLTags.StartListItem +
                    HTMLTags.EndTag +
                    HTMLTags.StartAnchor);
                Citation chapterCitation = Citation.InvalidCitation;
                await writer.Append(HTMLTags.HREF);
                if (title[Ordinals.first] == '#')
                {
                    chapterCitation = CitationConverter.FromString(title.Substring(Ordinals.second)).First();
                    await writer.Append(ChapterPage.pageURL +
                        HTMLTags.StartQuery +
                        ScripturePage.queryKeyStart +
                        Symbol.equal);
                    await writer.Append(chapterCitation.Start);
                    await writer.Append(HTMLTags.Ampersand +
                        ScripturePage.queryKeyEnd +
                        Symbol.equal);
                    await writer.Append(chapterCitation.End);
                    await writer.Append(HTMLTags.Ampersand +
                        ScripturePage.queryKeyNavigating +
                        Symbol.equal +
                        "true");
                }
                else
                {
                    await writer.Append(pageURL +
                        HTMLTags.StartQuery +
                        queryKeyName);
                    await writer.Append(paragraphs[index].Replace(" ", "_"));
                }
                await writer.Append(HTMLTags.Ampersand +
                    HTMLClasses.partial +
                    HTMLTags.OnClick +
                    JavaScriptFunctions.HandleTOCClick +
                    HTMLTags.EndTag);
                if (title[Ordinals.first] == '#')
                {
                    chapterCitation.LabelType = LabelTypes.Normal;
                    await CitationConverter.ToString(chapterCitation, writer);
                }
                else
                {
                    await writer.Append(title);
                }
                await writer.Append(HTMLTags.EndAnchor +
                    HTMLTags.EndListItem);
            }
            await writer.Append(HTMLTags.EndList);
        }

        public override string GetQueryInfo()
        {
            return HTMLTags.StartQuery + queryKeyName + name.Replace(" ", "_");
        }

        public async override Task LoadTOCInfo(HttpContext context)
        {
            paragraphs = await GetPageParagraphs();
            for (int index = Ordinals.first; index < paragraphs.Count; index++)
            {
                if ((paragraphs[index].Length > Number.nothing) &&
                    (paragraphs[index][Ordinals.first] == '='))
                {
                    mainHeadingIndex = index;
                    break;
                }
            }
        }

        public override async Task SetupNextPage()
        {
            string newName = await DataRepository.NextNavigation(name);
            if (newName.Contains("=="))
            {
                int parent = await ReadParentPageID(name);
                string uncle = await DataRepository.NextNavigation(parent);
                if (uncle.Contains("=="))
                {
                    int grandparent = await ReadParentPageID(parent);
                    string greatuncle = await DataRepository.NextNavigation(grandparent);
                    string cousin = await DataRepository.FirstNavigationParagraph(greatuncle);
                    newName = await DataRepository.FirstNavigationParagraph(cousin);
                    if (string.IsNullOrEmpty(newName) || (newName.Contains('#'))) newName = cousin;
                    if (string.IsNullOrEmpty(newName) || (newName.Contains('#'))) newName = greatuncle;
                }
                else
                {
                    newName = await DataRepository.FirstNavigationParagraph(uncle);
                    if (string.IsNullOrEmpty(newName) || (newName.Contains('#'))) newName = uncle;
                }
            }
            name = ((string.IsNullOrEmpty(newName)) || (newName.Contains("=="))) ?
                name :
                newName.Replace("_", " ");
        }

        private async Task<int> ReadParentPageID(string currentName)
        {
            return await DataRepository.NavigationParent(currentName);
        }
        private async Task<int> ReadParentPageID(int currentID)
        {
            return await DataRepository.NavigationParent(currentID);
        }

        public override async Task SetupPrecedingPage()
        {
            int currentIndex = await DataRepository.NavigationParagraphIndex(name);
            string newName;
            if (currentIndex == Ordinals.first)
            {
                int parent = await ReadParentPageID(name);
                string parentName = await DataRepository.NavigationName(parent);
                int parentIndex = await DataRepository.NavigationParagraphIndex(parentName);
                if (parentIndex == Ordinals.first)
                {
                    int grandparent = await ReadParentPageID(parent);
                    string grandparentName = await DataRepository.NavigationName(grandparent);
                    int grandparentIndex = await DataRepository.NavigationParagraphIndex(grandparentName);
                    int greatgrandparent = await ReadParentPageID(grandparent);
                    string greatuncle = await DataRepository.NavigationParagraph(greatgrandparent, grandparentIndex - 1);
                    string cousin = await ReadLastChild(greatuncle);
                    newName = await ReadLastChild(cousin);
                    if (string.IsNullOrEmpty(newName) || (newName.Contains('#'))) newName = cousin;
                    if (string.IsNullOrEmpty(newName) || (newName.Contains('#'))) newName = greatuncle;
                }
                else
                {
                    int grandparent = await ReadParentPageID(parent);
                    string uncle = await DataRepository.NavigationParagraph(grandparent, parentIndex - 1);
                    newName = await ReadLastChild(uncle);
                    if (string.IsNullOrEmpty(newName) || (newName.Contains('#'))) newName = uncle;
                }
            }
            else
            {
                int parent = await ReadParentPageID(name);
                newName = await DataRepository.NavigationParagraph(parent, currentIndex - 1);
            }
            name = (string.IsNullOrEmpty(newName)) ?
                name :
                newName.Replace("_", " ");

        }

        private async Task<string> ReadLastChild(string name)
        {
            int index = await DataRepository.NavigationHeadingIndex(name);
            return await DataRepository.NavigationParagraph(name, index - 1);
        }

        public override async Task SetupParentPage()
        {
            int id = await ReadParentPageID(name);
            name = (id == 0) ?
                name :
                (await DataRepository.NavigationName(id)).Replace("_", " ");
        }

        public override async Task HandleEditRequest(HttpContext context)
        {
            await WritePlainText(Writer.New(context.Response),
                context.Request.Query);
        }

        private async Task WritePlainText(HTMLWriter writer, IQueryCollection query)
        {
            name = query[requestNameQuery];
            var paragraphs = await GetPageParagraphs();
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
            var existingParagraphs = await GetPageParagraphs();
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
                await DataRepository.DeleteNavigationParagraph(id, i);
            }
            await RenderBody(Writer.New(context.Response));
        }

        private async Task AddParagraph(string paragraph, int index, int id)
        {
            await DataRepository.WriteNavigationParagraph(name, id, index, paragraph);
        }

        private async Task UpdateParagraph(string paragraph, int index, int id)
        {
            await DataRepository.DeleteNavigationParagraph(id, index);
            await DataRepository.WriteNavigationParagraph(name, id, index, paragraph);
        }
    }
}
