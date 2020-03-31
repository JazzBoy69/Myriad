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
    SetupModalPictures('."+HTMLClasses.scriptureComment+@"');

};
    </script>";
    }

    /*        
    SetupEditParagraph();
    SetThisVerseAsTarget();
    SetupPagination(); 
    HandleReadingView();
    ScrollToTarget();

    */

    public class TextPage : ScripturePage
    {
        public const string pageURL = "/Text";
        HTMLResponseWriter builder;
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
                    textSection.AddReadingViewSection(commentIDs[i]);
                }
            }
            else
            {
                textSection.AddTextSection(commentIDs[Ordinals.first], citation);
            }
        }

        private void Initialize()
        {
            builder = new HTMLResponseWriter(response);
            textSection = new TextSection(builder);
            commentIDs = GetCommentIDs(citation);
        }






        private List<int> GetCommentIDs(Citation citation)
        {
            var reader = SQLServerReaderProvider<int, int>.Reader(DataOperation.ReadCommentIDs,
                citation.CitationRange.StartID, citation.CitationRange.EndID);
            return reader.GetData<int>();
        }
    }
}
