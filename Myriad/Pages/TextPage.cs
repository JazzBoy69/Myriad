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
            builder = new HTMLResponseWriter(response);
            parser = new CommentParser(builder);
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
            List<int> commentIDs = GetCommentIDs(citation);
            bool readingView = commentIDs.Count > 1;
            if (readingView)
            {
                parser.AddTitle(GetTitle());
                for (var i = Ordinals.first; i < commentIDs.Count; i++)
                {
                    AddReadingViewSection(commentIDs[i]);
                }
            }
            else
            {
                AddTextAndComments(commentIDs[Ordinals.first]);
            }
        }

        private void AddTextAndComments(int commentID)
        {
            throw new NotImplementedException();
        }

        private void AddReadingViewSection(int commentID)
        {
            throw new NotImplementedException();
        }

        private List<int> GetCommentIDs(Citation citation)
        {
            return ReaderProvider.Reader()
                .GetData<int>(DataOperation.ReadCommentIDs, 
                citation.CitationRange.StartID, citation.CitationRange.EndID);
        }
    }
}
