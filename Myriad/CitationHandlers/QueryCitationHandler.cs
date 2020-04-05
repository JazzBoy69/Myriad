using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.CitationHandlers
{
    public class QueryCitationHandler : CitationHandler
    {
        protected override int IndexOfBook(string book)
        {
            string upper = book.ToUpper();
            return (Bible.QueryBibleNames.ContainsKey(upper)) ?
                Bible.NamesIndex[Bible.QueryBibleNames[upper]] :
                Result.notfound;
        }
    }
}
