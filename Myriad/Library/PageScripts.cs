﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Library
{
    public struct Scripts
    {
            public const string Text = @"
<script>
   window.onload = function () {
AddShortcut();
HandleGestures();
SetupIndex();
SetupPartialPageLoad();
HandleHiddenDetails();
SetIcons();
ScrollToTop();
};
    </script>";

        public const string Verse = @"
<script>
   window.onload = function () {
AddShortcut();
HandleGestures();
SetupIndex();
SetupPartialPageLoad();
HandleHiddenDetails();
SetupSuppressedParagraphs();
SetIcons();
ScrollToTop();
};
    </script>";


        /*        
        SetThisVerseAsTarget();
        SetupPagination(); 
        HandleReadingView();
        ScrollToTarget();
    */

        public const string Search = @"
<script>
   window.onload = function () {
AddShortcut();
HandleGestures();
SetupIndex();
SetSearchFieldText();
SetupPartialPageLoad();
ScrollToTop();
};
    </script>";
        }
}
