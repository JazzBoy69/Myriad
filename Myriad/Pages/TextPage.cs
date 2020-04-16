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

namespace Myriad.Pages
{
    public class TextPage : ScripturePage
    {
        public const string pageURL = "/Text";
        public const string precedingURL = "/Text-Preceding";
        public const string nextURL = "/Text-Next";
        public const string loadMainPane = "/Text-Load";
        HTMLWriter writer;
        List<int> commentIDs;
        TextSectionFormatter textSection;

        public void SetCitation(Citation citation)
        {
            this.citation = citation;
        }

        public override string GetURL()
        {
            return pageURL;
        }

        protected override CitationTypes GetCitationType()
        {
            return CitationTypes.Text;
        }

        protected async override Task WriteTitle(HTMLWriter writer)
        {
            await CitationConverter.ToString(citation, writer);
        }

        protected override string PageScripts()
        {
            return Scripts.Text;
        }

        public async override Task RenderBody(HTMLWriter writer)
        {
            this.writer = writer;
            await Initialize();
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
                await textSection.AddTextSection(commentIDs, Ordinals.first, citation, navigating, readingView);
            }
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
        }

        async internal void RenderMainPane(int startID, int endID)
        {
            citation = new Citation(startID, endID);
            await RenderBody(Writer.New(response));
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
                citation.CitationRange.EndID.ID);
            (int start, int end) = await reader.GetDatum<int, int>();
            citation = new Citation(start, end);
        }

        private async Task Initialize()
        {
            textSection = new TextSectionFormatter(writer);
            commentIDs = await GetCommentIDs(citation);
        }

        private async Task<List<int>> GetCommentIDs(Citation citation)
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadCommentIDs),
                citation.CitationRange.StartID.ID, citation.CitationRange.EndID.ID);
            return await reader.GetData<int>();
        }

        public override async Task AddTOC(HTMLWriter writer)
        {
            var ids = await GetCommentIDs(citation);
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
                var paragraphs = await TextSectionFormatter.ReadParagraphs(ids[index]);
                await writer.Append(paragraphs[Ordinals.first].Replace("==", ""));
                await writer.Append(HTMLTags.EndAnchor);
                await writer.Append(HTMLTags.EndListItem);
            }
            await writer.Append(HTMLTags.EndList);
        }

        public override async Task LoadTOCInfo(HttpContext context)
        {
            await LoadQueryInfo(context.Request.Query);
        }

    }
}
