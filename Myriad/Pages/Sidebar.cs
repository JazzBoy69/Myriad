using Feliciana.ResponseWriter;
using Microsoft.AspNetCore.Http;
using Myriad.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Myriad.Parser;

namespace Myriad.Pages
{
    public class Sidebar
    {
        internal static readonly string pageURL = "/Sidebar";
        public static async Task HandleRequest(HTMLWriter writer, IQueryCollection query)
        {
            Citation citation = GetCitation(query);
            citation.CitationType = CitationTypes.Text;
            await WriteHeader(writer, citation);
            //Add text of scriptures
        }

        private static async Task WriteHeader(HTMLWriter writer, Citation citation)
        {
            await PageFormatter.StartCitationLink(writer, citation);
            await CitationConverter.Append(writer, citation);
            await writer.Append(HTMLTags.EndAnchor);
        }

        private static Citation GetCitation(IQueryCollection query)
        {
            if (!int.TryParse(query[ScripturePage.queryKeyStart], out int start)) start = Result.notfound;
            if (!int.TryParse(query[ScripturePage.queryKeyEnd], out int end)) end = Result.notfound;
            Citation citation = new Citation(start, end);
            return citation;
        }

    }
}
