using System.Collections.Generic;
using System.Threading.Tasks;
using Myriad.Library;
using Myriad.Data;
using Myriad.Parser;
using Myriad.Writer;

namespace Myriad.Pages
{
    public struct TextHTML
    {
        public const string TextScripts = @"
<script>
   window.onload = function () {
    AddShortcut();
    SetupIndex();
SetupPartialPageLoad();
};
    </script>";
    }

    /*        
    SetThisVerseAsTarget();
    SetupPagination(); 
    HandleReadingView();
    ScrollToTarget();

    */

    public class TextPage : ScripturePage
    {
        public const string pageURL = "/Text";
        public const string precedingURL = "/Text-Preceding";
        public const string nextURL = "/Text-Next";
        public const string loadMainPane = "/Text-Load";
        HTMLWriter writer;
        List<int> commentIDs;
        TextSection textSection;

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
            return TextHTML.TextScripts;
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
                    await textSection.AddReadingViewSection(commentIDs[i]);
                }
            }
            else
            {
                await textSection.AddTextSection(commentIDs[Ordinals.first], citation);
            }
            await AddPageTitleData(writer);
        }

        async internal void RenderMainPane(int startID, int endID)
        {
            citation = new Citation(startID, endID);
            await RenderBody(WriterReference.New(response));
        }

        public override void SetupNextPage()
        {
            var reader = DataReaderProvider<int>.Reader(DataOperation.ReadNextCommentRange,
                citation.CitationRange.EndID);
            (int start, int end) = reader.GetDatum<int, int>();
            reader.Close();
            citation = new Citation(start, end);
        }

        public override void SetupPrecedingPage()
        {
            var reader = DataReaderProvider<int>.Reader(DataOperation.ReadPrecedingCommentRange,
                citation.CitationRange.EndID);
            (int start, int end) = reader.GetDatum<int, int>();
            reader.Close();
            citation = new Citation(start, end);
        }

        private void Initialize()
        {
            textSection = new TextSection(writer);
            commentIDs = GetCommentIDs(citation);
        }

        private List<int> GetCommentIDs(Citation citation)
        {
            var reader = DataReaderProvider<int, int>.Reader(DataOperation.ReadCommentIDs,
                citation.CitationRange.StartID, citation.CitationRange.EndID);
            return reader.GetData<int>();
        }

        public override Task AddTOC(HTMLWriter writer)
        {
            throw new System.NotImplementedException();
        }

        public override Task LoadTOCInfo()
        {
            throw new System.NotImplementedException();
        }
    }
}
