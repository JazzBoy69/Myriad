using Myriad.Parser;
using System;
using System.Collections.Generic;

namespace Myriad.Pages
{
    internal static class PageReferrer
    {
        internal static ChapterPage chapterPage = new ChapterPage();
        internal static TextPage textPage = new TextPage();
        internal static VersePage versePage = new VersePage();
        internal static ScriptureErrorPage errorPage = new ScriptureErrorPage();
        internal static ScripturePage GetPage(CitationTypes citationType)
        {
            switch (citationType)
            {
                case CitationTypes.Chapter:
                    return chapterPage;
                case CitationTypes.Text:
                    return textPage;
                case CitationTypes.Verse:
                    return versePage;
                default:
                    return errorPage;
            }
        }
    }
}