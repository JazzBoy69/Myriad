using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.ResponseWriter;
using Feliciana.HTML;
using Myriad.Library;

namespace Myriad.Pages
{
    public abstract class PaginationPage : CommonPage
    {
        public abstract Task SetupParentPage();
        public abstract Task SetupNextPage();
        public abstract Task SetupPrecedingPage();
        protected async Task AddPagination(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartDivWithID +
                HTMLClasses.paginate + HTMLTags.CloseQuote +
                HTMLTags.Class + HTMLClasses.hidden +
                HTMLTags.CloseQuoteEndTag + HTMLTags.EndDiv);
        }
    }
}
