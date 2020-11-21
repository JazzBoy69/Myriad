using Feliciana.Library;
using Myriad.Library;

namespace Myriad.CitationHandlers
{
    public class QueryCitationHandler2 : CitationHandler2
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
