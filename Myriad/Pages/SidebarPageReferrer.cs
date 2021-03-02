using System.Collections.Generic;
using Myriad.Library;

namespace Myriad.Pages
{
    internal static class SidebarPageReferrer
    {
        internal static Dictionary<CitationTypes, string> URLs = new Dictionary<CitationTypes, string>()
        {
            { CitationTypes.Chapter, ChapterPage.pageURL },
            { CitationTypes.Text, Sidebar.pageURL },
            { CitationTypes.Verse, VersePage.pageURL },
            { CitationTypes.Invalid, IndexPage.pageURL }
        };
    }
}
