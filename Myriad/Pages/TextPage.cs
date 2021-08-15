using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
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
        TextSections textSections = new TextSections();
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
            citation.LabelType = LabelTypes.Normal;
            await CitationConverter.ToString(citation, writer);
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
            await Initialize();
            if (textSections.ReadingView)
            {
                await writer.Append(HTMLTags.StartMainHeader);
                await WriteTitle(writer);
                await writer.Append(HTMLTags.EndMainHeader);
                await TextParagraph.AddText(writer, textSections);
                await writer.Append(HTMLTags.StartDivWithID +
                    HTMLClasses.expandedText +
                    HTMLTags.CloseQuote +
                    HTMLTags.Class +
                    HTMLClasses.hidden +
                    HTMLTags.CloseQuoteEndTag);
                await textSections.AddSections(writer);
                await writer.Append(HTMLTags.EndDiv);
            }
            else
            {
                await textSections.AddTextSection(writer);
                await AddEditPageData(writer);
            }
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
            await AddPagination(writer);
            await AddChronoLink(writer);
        }

        private async Task AddChronoLink(HTMLWriter writer)
        {
            int chronoID = await Chrono.GetIDFromCitation(citation);
            if (chronoID > Number.nothing)
            {
                await writer.Append(HTMLTags.StartDivWithID +
                HTMLClasses.chrono + HTMLTags.CloseQuote +
                HTMLTags.Class + HTMLClasses.hidden +
                HTMLTags.CloseQuoteEndTag);
                await writer.Append(chronoID);
                await writer.Append(HTMLTags.EndDiv);
            }
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
            {
                await DataRepository.DeleteCommentParagraph(id, Ordinals.first);
                await DataRepository.WriteCommentParagraph(id, Ordinals.first, newParagraphs[Ordinals.first]);
            }
            TextSections textSection = new TextSections();
            textSection.navigating = true;
            textSection.sourceCitation = citation;
            await textFormatter.StartTextSection(id, textSection);
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
                    await DataRepository.DeleteCommentParagraph(id, i);
            }

            await textFormatter.EndCommentSection();
            commentIDs = new List<int>() { id };
            await AddEditPageData(writer);
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
            await AddPagination(writer);
            await AddChronoLink(writer);
        }

        internal async Task WritePlainText(HTMLWriter writer, IQueryCollection query)
        {
            string idString = query[queryKeyID];
            int id = Numbers.Convert(idString);
            var paragraphs = TextSectionFormatter.ReadParagraphs(id);
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
            await writer.Append(citation.Start);
            await writer.Append(HTMLTags.Ampersand +
                queryKeyEnd +
                Symbol.equal);
            await writer.Append(citation.End);
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
            (int start, int end) = await DataRepository.NextCommentRange(citation.End);
            citation = new Citation(start, end);
        }

        public override async Task SetupPrecedingPage()
        {
            (int start, int end) = await DataRepository.PrecedingCommentRange(citation.Start);
            citation = new Citation(start, end);
        }

        private async Task Initialize()
        {
            commentIDs = await DataRepository.CommentIDsInRange(citation.Start, citation.End);
            textSections.SetHighlightRange(citation);
            textSections.navigating = navigating;
            textSections.sourceCitation = citation;
            textSections.CommentIDs = commentIDs;
        }

        public override async Task WriteTOC(HTMLWriter writer)
        {
            var ids = await DataRepository.CommentIDsInRange(citation.Start, citation.End);
            if (ids.Count < 2) return;
            await writer.Append(HTMLTags.StartList);
            await writer.Append(HTMLTags.ID);
            await writer.Append(HTMLClasses.toc);
            await writer.Append(HTMLTags.Class);
            await writer.Append(HTMLClasses.visible);
            await writer.Append(HTMLTags.CloseQuoteEndTag);
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
                string heading = await DataRepository.CommentHeading(ids[index]);
                await writer.Append(heading.Replace("==", ""));
                await writer.Append(HTMLTags.EndAnchor);
                await writer.Append(HTMLTags.EndListItem);
            }
            await writer.Append(HTMLTags.EndList);
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
