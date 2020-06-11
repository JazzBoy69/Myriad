using Feliciana.ResponseWriter;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.Data;
using Myriad.Library;
using Myriad.Data;
using Myriad.Formatter;
using Myriad.Parser;

namespace Myriad.Pages
{
    public class Chrono : PaginationPage
    {
        public const string ChronoScripts = @"{
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
        internal const string pageURL = "/Chrono";
        public const string editURL = "/Edit/Chrono";
        internal const string queryKeyID = "id";
        internal const string queryKeyTGStart = "tgstart";
        internal const string queryKeyTGEnd = "tgend";
        public const string queryKeyNavigating = "navigating";
        protected Citation targetCitation;
        protected int id;
        protected bool navigating;
        HTMLWriter writer; 
        List<int> commentIDs;
        List<string> articleParagraphs;
        TextSectionFormatter textSection;
        PageParser parser;
        public override string GetQueryInfo()
        {
            StringBuilder info = new StringBuilder();
            if (id>Result.nothing)
            {
                info.Append(HTMLTags.StartQuery + queryKeyID + Symbol.equal);
                info.Append(id);
            }
            if ((targetCitation != null) && (targetCitation.CitationRange.Valid))
            {
                if (id > Result.nothing) 
                    info.Append(HTMLTags.Ampersand);
                else
                    info.Append(HTMLTags.StartQuery);
                info.Append(queryKeyTGStart + Symbol.equal);
                info.Append(targetCitation.CitationRange.StartID);
                info.Append(HTMLTags.Ampersand + queryKeyTGEnd + Symbol.equal);
                info.Append(targetCitation.CitationRange.EndID);
            }
            return (navigating) ?
                info.Append(HTMLTags.Ampersand + queryKeyNavigating + "=true").ToString() :
                info.ToString();
        }

        public override string GetURL()
        {
            return pageURL;
        }

        public override Task HandleAcceptedEdit(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public override Task HandleEditRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid()
        {
            return (id > Number.nothing) || ((targetCitation != null) && targetCitation.CitationRange.Valid);
        }

        public override async Task LoadQueryInfo(IQueryCollection query)
        {
            if (!int.TryParse(query[queryKeyTGStart], out int start)) start = Result.notfound;
            if (!int.TryParse(query[queryKeyTGEnd], out int end)) end = Result.notfound;
            targetCitation = new Citation(start, end);
            int.TryParse(query[queryKeyID], out int id);
            this.id = (id>Number.nothing) ? id : await GetIDFromCitation(targetCitation);
            navigating = query.ContainsKey(queryKeyNavigating);
        }

        internal static async Task<int> GetIDFromCitation(Citation citation)
        {
            var commentIDs = GetCommentIDs(citation);
            if (commentIDs.Count < 1) return Number.nothing;
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadChronoChapterID),
                commentIDs[Ordinals.first]);
            int chapterID = await reader.GetDatum<int>();
            reader.Close();
            return chapterID;
        }

        private static List<int> GetCommentIDs(Citation citation)
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadCommentIDs),
                citation.CitationRange.StartID.ID, citation.CitationRange.EndID.ID);
            var results = reader.GetData<int>();
            reader.Close();
            return results;
        }
        public override async Task LoadTOCInfo(HttpContext context)
        {
            await LoadQueryInfo(context.Request.Query);
        }

        public override async Task RenderBody(HTMLWriter writer)
        {
            this.writer = writer;
            Initialize();
            await writer.Append(HTMLTags.StartMainHeader);
            await WriteTitle(writer);
            await writer.Append(HTMLTags.EndMainHeader);
            await WriteChapterComment();
            for (int i = Ordinals.first; i < commentIDs.Count; i++)
            {
                await textSection.AddTextSection(commentIDs, i, targetCitation, navigating, true);
            }
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
            await AddTOCButton(writer);
            await AddPagination(writer);
        }
        private void Initialize()
        {
            textSection = new TextSectionFormatter(writer);
            parser = new PageParser(writer);
            commentIDs = GetCommentIDs();
            GetArticleParagraphs();
        }
        private async Task WriteChapterComment()
        {
            if (articleParagraphs.Count == Number.nothing) return;
            parser.SetParagraphInfo(ParagraphType.Chrono, id);
            await writer.Append(
                HTMLTags.StartDivWithID +
                HTMLClasses.chaptercomments +
                HTMLTags.CloseQuote +
                HTMLTags.Class +
                HTMLClasses.textquote +
                HTMLTags.CloseQuoteEndTag);
            parser.SetStartHTML(HTMLTags.StartParagraph);
            parser.SetEndHTML(HTMLTags.EndParagraph);
            for (int i = Ordinals.first; i < articleParagraphs.Count; i++)
            {
                await parser.ParseParagraph(articleParagraphs[i], i);
            }
            await writer.Append(HTMLTags.EndDiv);
        }
        private void GetArticleParagraphs()
        {
            if (id == Number.nothing)
            {
                articleParagraphs = new List<string>();
                return;
            }
            var paragraphReader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadChronoArticle),
                id);
            var paragraphs = paragraphReader.GetData<string>();
            articleParagraphs = new List<string>();
            for (int i = Ordinals.first; i < paragraphs.Count; i++)
                if (!string.IsNullOrWhiteSpace(paragraphs[i])) articleParagraphs.Add(paragraphs[i]);
            paragraphReader.Close();
        }
        public override Task SetupNextPage()
        {
            throw new NotImplementedException();
        }

        public override Task SetupParentPage()
        {
            throw new NotImplementedException();
        }

        public override Task SetupPrecedingPage()
        {
            throw new NotImplementedException();
        }

        public override async Task WriteTOC(HTMLWriter writer)
        {
            var ids = GetCommentIDs();
            if (ids.Count < 2) return;
            await writer.Append(HTMLTags.StartList);
            await writer.Append(HTMLTags.ID);
            await writer.Append(HTMLClasses.toc);
            await writer.Append(HTMLTags.Class);
            await writer.Append(HTMLClasses.visible);
            await writer.Append(HTMLTags.CloseQuoteEndTag);
            var reader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadCommentTitle),
                -1);

            for (int index = Ordinals.first; index < ids.Count; index++)
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
                reader.SetParameter(ids[index]);
                string heading = await reader.GetDatum<string>();
                await writer.Append(heading.Replace("==", ""));
                await writer.Append(HTMLTags.EndAnchor);
                await writer.Append(HTMLTags.EndListItem);
            }
            await writer.Append(HTMLTags.EndList);
            reader.Close();
        }

        private List<int> GetCommentIDs()
        {
            if (id == Number.nothing) return new List<int>();
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadChronoIDs),
                id);
            var result = reader.GetData<int>();
            reader.Close();
            return result;
        }

        protected override string PageScripts()
        {
            return ChronoScripts;
        }

        internal override async Task WriteTitle(HTMLWriter writer)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadChronoTitle),
                id);
            string title = await reader.GetDatum<string>();
            reader.Close();
            await writer.Append(title);
        }
    }
}
