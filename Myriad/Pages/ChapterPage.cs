using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Feliciana.ResponseWriter;
using Feliciana.Library;
using Feliciana.Data;
using Feliciana.HTML;
using Myriad.Parser;
using Myriad.Data;
using Myriad.Formatter;
using Myriad.Library;
using System.IO.Pipelines;

namespace Myriad.Pages
{
    public class ChapterPage : ScripturePage
    {
        public const string pageURL = "/Chapter";
        public const string editURL = "/Edit/Article";
        HTMLWriter writer;
        List<int> commentIDs;
        PageParser parser;
        List<string> headings;
        TextSections textSections = new TextSections();
        int articleID;
        List<string> articleParagraphs;
        string indexPageName;
        Citation chapterCitation;

        public ChapterPage()
        {
        }

        internal override async Task WriteTitle(HTMLWriter writer)
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
            if (!string.IsNullOrEmpty(indexPageName))
            {
                IndexPage indexPage = new IndexPage();
                indexPage.SetName(indexPageName);
                indexPage.SetResponse(response);
                await indexPage.RenderBody(writer);
                return;
            }
            this.writer = writer;
            await Initialize();
            await writer.Append(HTMLTags.StartMainHeader);
            await WriteTitle(writer);
            await writer.Append(HTMLTags.EndMainHeader);
            await WriteChapterComment();
            textSections.SetHighlightRange(citation);
            await TextParagraph.AddText(writer, textSections);
            await writer.Append(HTMLTags.StartDivWithID +
                HTMLClasses.expandedText +
                HTMLTags.CloseQuote+
                HTMLTags.Class +
                HTMLClasses.hidden +
                HTMLTags.CloseQuoteEndTag);
            await textSections.AddSections(writer);
            await writer.Append(HTMLTags.EndDiv);
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
            await AddTOCButton(writer);
            await AddPagination(writer);
            await AddEditPageData(writer);
        }
        private async Task AddEditPageData(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartDivWithID +
                HTMLClasses.editdata + HTMLTags.CloseQuote +
                HTMLTags.Class +
                HTMLClasses.hidden +
                HTMLTags.CloseQuoteEndTag +
                editURL + HTMLTags.StartQuery +
                ArticlePage.queryKeyTitle + Symbol.equal);
            await CitationConverter.AppendChapterTitle(writer, citation.citationRange);
            await writer.Append(HTMLTags.EndDiv);
        }

        private async Task Initialize()
        {
            chapterCitation = GetChapterCitation();
            parser = new PageParser(writer);
            commentIDs = await DataRepository.CommentIDsInRange(chapterCitation.Start, chapterCitation.End);
            await GetArticleParagraphs();
            textSections.navigating = navigating;
            textSections.sourceCitation = chapterCitation;
            textSections.SetHighlightRange(citation);
            textSections.CommentIDs = commentIDs;
        }

        private async Task GetArticleParagraphs()
        {
            string label = ChapterLabel();
            articleID = await DataRepository.ArticleID(label);
            if (articleID == Number.nothing)
            {
                articleParagraphs = new List<string>();
                return;
            }
            articleParagraphs = await DataRepository.Article(articleID);
        }

        internal string ChapterLabel()
        {
            StringBuilder result = new StringBuilder(Bible.AbbreviationsTitleCase[citation.Book]);
            result.Append(' ');
            if (TextReference.IsShortBook(citation.Book))
            {
                result.Append(" (Bible book)");
                return result.ToString();
            }
            result.Append(citation.FirstChapter);
            return result.ToString();
        }

        private Citation GetChapterCitation()
        {
            KeyID start = new KeyID(citation.Book, citation.FirstChapter, 0);
            KeyID end = new KeyID(citation.Book, citation.FirstChapter,
                Bible.Chapters[citation.Book][citation.FirstChapter], KeyID.MaxWordIndex);
            return new Citation(start, end);
        }

        private async Task WriteChapterComment()
        {
            if (articleParagraphs.Count == Number.nothing) return;
            parser.SetParagraphInfo(ParagraphType.Article, articleID);
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

        public override string GetURL()
        {
            return pageURL;
        }

        protected override CitationTypes GetCitationType()
        {
            return CitationTypes.Chapter;
        }


        public override async Task SetupNextPage()
        {
            var chapterCitation = GetChapterCitation();
            (int start, int end) = await DataRepository.NextCommentRange(chapterCitation.End);
            citation = new Citation(start, end);
            citation.CitationType = CitationTypes.Chapter;
        }

        public override async Task SetupPrecedingPage()
        {
            var chapterCitation = GetChapterCitation();
            (int start, int end) = await DataRepository.PrecedingCommentRange(chapterCitation.Start);
            citation = new Citation(start, end);
            citation.CitationType = CitationTypes.Chapter;
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
            citation = new Citation(citation.Book, citation.FirstChapter, 
                CitationRange.AllVerses);
        }

        public override async Task SetupParentPage()
        {
            indexPageName = 
                await DataRepository.ChapterNavigationName(citation.Book, citation.FirstChapter);
        }

        public override Task HandleEditRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public override Task HandleAcceptedEdit(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
