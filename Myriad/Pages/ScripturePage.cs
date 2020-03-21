using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Parser;

namespace Myriad.Pages
{
    public abstract class ScripturePage : CommonPage
    {
        internal const string queryKeyTGStart = "tgstart=";
        internal const string queryKeyTGEnd = "tgend=";
        public void AppendQuery(HTMLResponse builder, Citation citation)
        {
            builder.Append("start=");
            builder.Append(citation.CitationRange.StartID);
            builder.Append("&end=");
            builder.Append(citation.CitationRange.EndID);
        }
    }
}
