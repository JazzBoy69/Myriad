using System.Collections.Generic;
using Myriad.Library;

namespace Myriad.Pages
{
    internal static class PageReferrer
    {
        internal static ChapterPage chapterPage = new ChapterPage();
        internal static TextPage textPage = new TextPage();
        internal static VersePage versePage = new VersePage();
        internal static ScriptureErrorPage errorPage = new ScriptureErrorPage();

        internal static Dictionary<CitationTypes, string> URLs = new Dictionary<CitationTypes, string>()
        {
            { CitationTypes.Chapter, ChapterPage.pageURL },
            { CitationTypes.Text, TextPage.pageURL },
            { CitationTypes.Verse, VersePage.pageURL },
            { CitationTypes.Invalid, IndexPage.pageURL }
        };
        internal static void AppendQuery(HTMLWriter writer, Citation citation)
        {
            writer.Append("start=");
            writer.Append(citation.CitationRange.StartID);
            writer.Append("&end=");
            writer.Append(citation.CitationRange.EndID);
        }
    }
}