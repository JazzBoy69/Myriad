using System;
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
    SetupIndex();
SetupPartialPageLoad();
ScrollToTarget();
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
    SetupIndex();
SetSearchFieldText();
SetupPartialPageLoad();
ScrollToTop();
};
    </script>";
        }
}
