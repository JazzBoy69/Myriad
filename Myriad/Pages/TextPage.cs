using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Feliciana.Data;
using Myriad.Library;
using Myriad.Data;
using Myriad.Parser;
using Myriad.Formatter;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace Myriad.Pages
{
    public class TextPage : ScripturePage
    {
        public const string pageURL = "/Text";
        public const string editURL = "/Edit/Text";
        HTMLWriter writer;
        List<int> commentIDs;
        TextSectionFormatter textSection;
        public const string queryKeyID = "ID";

        public override string GetURL()
        {
            if (citation.CitationType == CitationTypes.Chapter) return ChapterPage.pageURL;
            return pageURL;
        }

        protected override CitationTypes GetCitationType()
        {
            return (citation == null) ?
               CitationTypes.Text :
               citation.CitationType;
        }

        internal async override Task WriteTitle(HTMLWriter writer)
        {
            await CitationConverter.ToLongString(citation, writer);
        }

        protected override string PageScripts()
        {
            return Scripts.Text;
        }

        public async override Task RenderBody(HTMLWriter writer)
        {
            if (citation.CitationType == CitationTypes.Chapter)
            {
                ChapterPage chapterPage = new ChapterPage();
                chapterPage.SetCitation(citation);
                chapterPage.SetResponse(response);
                await chapterPage.RenderBody(writer);
                return;
            }
            this.writer = writer;
            Initialize();
            bool readingView = commentIDs.Count > 1;
            if (readingView)
            {
                await writer.Append(HTMLTags.StartMainHeader);
                await WriteTitle(writer);
                await writer.Append(HTMLTags.EndMainHeader);
                for (var i = Ordinals.first; i < commentIDs.Count; i++)
                {
                    await textSection.AddTextSection(commentIDs, i, citation, navigating, readingView);
                }
            }
            else
            {
                //await AddTimeLine(writer, commentIDs[Ordinals.first]);
                await textSection.AddTextSection(commentIDs, Ordinals.first, citation, navigating, readingView);
                await AddEditPageData(writer);
            }
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
            await AddPagination(writer);
            int chronoID = await Chrono.GetIDFromCitation(citation);
            if (chronoID > Number.nothing)
            {
                await AddChronoLink(writer, chronoID);
            }
        }

        private async Task AddTimeLine(HTMLWriter writer, int commentID)
        {
            int chronoID = await GetChronoID(commentID);
            await Timeline.Write(writer, chronoID);
        }

        private async Task<int> GetChronoID(int commentID)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadChronoChapterID), commentID);
            int result = await reader.GetDatum<int>();
            reader.Close();
            return result;
        }

        private async Task AddChronoLink(HTMLWriter writer, int chronoID)
        {
            await writer.Append(HTMLTags.StartDivWithID +
                HTMLClasses.chrono + HTMLTags.CloseQuote +
                HTMLTags.Class + HTMLClasses.hidden +
                HTMLTags.CloseQuoteEndTag);
            await writer.Append(chronoID);
            await writer.Append(HTMLTags.EndDiv);
        }
        internal async Task UpdateComment(HTMLWriter writer, IQueryCollection query, string text)
        {
            string idString = query[queryKeyID];
            int id = Numbers.Convert(idString);
            if (!int.TryParse(query[queryKeyStart], out int start)) start = Result.notfound;
            if (!int.TryParse(query[queryKeyEnd], out int end)) end = Result.notfound;
            citation = new Citation(start, end);
            citation.CitationType = CitationTypes.Text;
            var paragraphs = TextSectionFormatter.ReadParagraphs(id);
            var newParagraphs = text.Split(Symbols.linefeedArray, StringSplitOptions.RemoveEmptyEntries).ToList();
            var textFormatter = new TextSectionFormatter(writer);
            textFormatter.SetHeading(newParagraphs[Ordinals.first]);
            ArticleParagraph heading = new ArticleParagraph(id, Ordinals.first, newParagraphs[Ordinals.first]);
            if (newParagraphs[Ordinals.first] != paragraphs[Ordinals.first]) 
                await EditParagraph.WriteParagraphToDatabase(heading);
            await textFormatter.StartTextSection(id, citation, true, false);
            for (int i = Ordinals.second; i < newParagraphs.Count; i++)
            {
                ArticleParagraph commentParagraph = new ArticleParagraph(id, i, newParagraphs[i]);
                if ((i<paragraphs.Count) && (newParagraphs[i] != paragraphs[i]))
                {
                    await EditParagraph.UpdateCommentParagraph(textFormatter.Parser, commentParagraph);
                    continue;
                }
                if (i >= paragraphs.Count)
                {
                    await EditParagraph.AddCommentParagraph(textFormatter.Parser, commentParagraph);
                    continue;
                }
                await textFormatter.ParseParagraph(newParagraphs[i], i);
            }
            if (newParagraphs.Count < paragraphs.Count)
            {
                for (int i = newParagraphs.Count; i < paragraphs.Count; i++)
                    await EditParagraph.DeleteCommentParagraph(id, i);
            }

            await textFormatter.EndCommentSection();
            commentIDs = new List<int>() { id };
            await AddEditPageData(writer);
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
            await AddPagination(writer);
            int chronoID = await Chrono.GetIDFromCitation(citation);
            if (chronoID > Number.nothing)
            {
                await AddChronoLink(writer, chronoID);
            }
        }

        internal async Task WritePlainText(HTMLWriter writer, IQueryCollection query)
        {
            string idString = query[queryKeyID];
            int id = Numbers.Convert(idString);
            var paragraphs = TextSectionFormatter.ReadParagraphs(id);
            for (int i = Ordinals.first; i < paragraphs.Count; i++)
            {
                await writer.Append(paragraphs[i]);
                await writer.Append(Symbol.lineFeed);
            }
        }

        private async Task AddEditPageData(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartDivWithID +
                HTMLClasses.editdata + HTMLTags.CloseQuote +
                HTMLTags.Class +
                HTMLClasses.hidden +
                HTMLTags.CloseQuoteEndTag +
                editURL + HTMLTags.StartQuery +
                queryKeyID + Symbol.equal);
            await writer.Append(commentIDs[Ordinals.first]);
            await writer.Append(HTMLTags.Ampersand +
                queryKeyStart +
                Symbol.equal);
            await writer.Append(citation.CitationRange.StartID.ID);
            await writer.Append(HTMLTags.Ampersand +
                queryKeyEnd +
                Symbol.equal);
            await writer.Append(citation.CitationRange.EndID.ID);
            await writer.Append(HTMLTags.EndDiv);
        }

        public override Task SetupParentPage()
        {
            targetCitation = citation;
            citation.CitationType = CitationTypes.Chapter;
            return Task.CompletedTask;
        }
        public override async Task SetupNextPage()
        {
            var reader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadNextCommentRange),
                citation.CitationRange.EndID.ID);
            (int start, int end) = await reader.GetDatum<int, int>();
            reader.Close();
            citation = new Citation(start, end);
        }

        public override async Task SetupPrecedingPage()
        {
            var reader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadPrecedingCommentRange),
                citation.CitationRange.StartID.ID);
            (int start, int end) = await reader.GetDatum<int, int>();
            citation = new Citation(start, end);
            reader.Close();
        }

        private void Initialize()
        {
            textSection = new TextSectionFormatter(writer);
            commentIDs = GetCommentIDs(citation);
            if (targetCitation != null) textSection.SetTargetCitation(targetCitation);
        }

        private List<int> GetCommentIDs(Citation citation)
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadCommentIDs),
                citation.CitationRange.StartID.ID, citation.CitationRange.EndID.ID);
            var results = reader.GetData<int>();
            reader.Close();
            return results;
        }

        public override async Task WriteTOC(HTMLWriter writer)
        {
            var ids = GetCommentIDs(citation);
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

        public override async Task LoadTOCInfo(HttpContext context)
        {
            await LoadQueryInfo(context.Request.Query);
        }

        public override async Task HandleEditRequest(HttpContext context)
        {
            await WritePlainText(Writer.New(context.Response),
                context.Request.Query);
        }

        public override async Task HandleAcceptedEdit(HttpContext context)
        {
            context.Request.Form.TryGetValue("text", out var text);
            await UpdateComment(Writer.New(context.Response),
                context.Request.Query, text.ToString());
        }
    }
}
