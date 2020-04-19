using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Feliciana.ResponseWriter;
using Feliciana.Library;
using Feliciana.Data;
using Feliciana.HTML;
using Myriad.Parser;
using Myriad.Data;
using Myriad.Formatter;
using Myriad.Library;

namespace Myriad.Pages
{
    public class ChapterPage : ScripturePage
    {
        public const string pageURL = "/Chapter";
        HTMLWriter writer;
        List<int> commentIDs;
        TextSectionFormatter textSection;
        List<string> headings;

        //todo implement chapter page
        internal ChapterPage()
        {
        }

        protected override async Task WriteTitle(HTMLWriter writer)
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
                await textSection.AddTextSection(commentIDs, Ordinals.first, citation, navigating, readingView);
            }
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
        }

        private void Initialize()
        {
            textSection = new TextSectionFormatter(writer);
            commentIDs = GetCommentIDs(citation);
        }
        private List<int> GetCommentIDs(Citation citation)
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadCommentIDs),
                citation.CitationRange.StartID.ID, citation.CitationRange.EndID.ID);
            return reader.GetData<int>();
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
            reader.Close();
        }

        public override async Task AddTOC(HTMLWriter writer)
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

        public override Task SetupParentPage()
        {
            throw new NotImplementedException();
        }
    }
}
