﻿using System.Collections.Generic;
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
    SetupModalPictures('."+HTMLClasses.scriptureComment+ @"');
    SetupPagination(); 
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

        protected override string GetTitle()
        {
            return CitationConverter.ToString(citation);
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
                writer.Append(HTMLTags.StartMainHeader);
                writer.Append(GetTitle());
                writer.Append(HTMLTags.EndMainHeader);
                for (var i = Ordinals.first; i < commentIDs.Count; i++)
                {
                    textSection.AddReadingViewSection(commentIDs[i]);
                }
            }
            else
            {
                textSection.AddTextSection(commentIDs[Ordinals.first], citation);
            }
            await AddPageTitleData();
        }

        internal void RenderMainPane(int startID, int endID)
        {
            citation = new Citation(startID, endID);
            RenderBody(WriterReference.New(response));
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
    }
}
