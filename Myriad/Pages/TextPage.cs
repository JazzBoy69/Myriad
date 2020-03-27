using Microsoft.AspNetCore.Http;
using Myriad.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;
using Myriad.Data;

namespace Myriad.Pages
{
    public struct TextHTML
    {
        public const string TextScripts = @"
<script>
   window.onload = function () {
    shortcut.add('Ctrl+F10', function () {
         document.getElementById('searchField').focus();
    });
    SetupIndex();
    SetupEditParagraph();
    SetThisVerseAsTarget();
    SetupPagination();
    SetupModalPictures();
    HandleReadingView();
    HandleTabClicks();
    ScrollToTarget();
};
    </script>";
    }

    public class TextPage : ScripturePage
    {
        public const string pageURL = "/Text";
        CommentParser parser;
        HTMLResponseWriter builder;
        List<int> commentIDs;
        List<string> paragraphs;

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

        protected override void RenderBody()
        {
            Initialize();
            bool readingView = commentIDs.Count > 1;
            if (readingView)
            {
                builder.Append(HTMLTags.StartMainHeader);
                builder.Append(GetTitle());
                builder.Append(HTMLTags.EndMainHeader);
                for (var i = Ordinals.first; i < commentIDs.Count; i++)
                {
                    AddReadingViewSection(commentIDs[i]);
                }
            }
            else
            {
                List<(int start, int end)> idRanges = ReadLinks(commentIDs[Ordinals.first]);

                paragraphs = ReadParagraphs(commentIDs[Ordinals.first]);
                if (idRanges.Count > 1) AddTextTabs(idRanges);
                else AddText(idRanges[Ordinals.first]);
                AddComment();
            }
        }

        public List<string> ReadParagraphs(int commentID)
        {
            var reader = ReaderProvider<int>.Reader(DataOperation.ReadArticle,
                commentID);
            return reader.GetData<string>();
        }

        public List<(int start, int end)> ReadLinks(int commentID)
        {
            var reader = ReaderProvider<int>.Reader(DataOperation.ReadCommentLinks,
                commentID);
            return reader.GetData<int, int>();
        }
        private void Initialize()
        {
            builder = new HTMLResponseWriter(response);
            parser = new CommentParser(builder);
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            commentIDs = GetCommentIDs(citation);
        }

        private void AddComment()
        {
            throw new NotImplementedException();
        }

        private void AddText((int start, int end) textRange)
        {
            builder.Append(HTMLTags.StartMainHeader);
            Citation citation = new Citation(textRange.start, textRange.end);
            builder.Append(" (");
            CitationConverter.Append(builder, citation);
            builder.Append(")");
            builder.Append(HTMLTags.EndMainHeader);
            List<Keyword> keywords = ReadKeywords(citation);
            //TODO add text body
        }

        public List<Keyword> ReadKeywords(Citation citation)
        {
            var reader = ReaderProvider<int, int>.Reader(DataOperation.ReadKeywords,
                citation.CitationRange.StartID, citation.CitationRange.EndID);
            return reader.GetClassData<Keyword>();
        }

        private void AddTextTabs(List<(int start, int end)> idRanges)
        {
            throw new NotImplementedException();
        }

        private void AddReadingViewSection(int commentID)
        {
            throw new NotImplementedException();
        }

        private List<int> GetCommentIDs(Citation citation)
        {
            var reader = ReaderProvider<int, int>.Reader(DataOperation.ReadCommentIDs,
                citation.CitationRange.StartID, citation.CitationRange.EndID);
            return reader.GetData<int>();
        }
    }
}
