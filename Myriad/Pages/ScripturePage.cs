using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.Data;
using Feliciana.HTML;
using Myriad.Library;
using Myriad.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Myriad.Pages
{
    public abstract class ScripturePage : PaginationPage
    {
        internal const string queryKeyTGStart = "tgstart";
        internal const string queryKeyTGEnd = "tgend";
        public const string queryKeyStart = "start";
        public const string queryKeyEnd = "end";
        public const string queryKeyWord = "word";
        public const string queryKeyNavigating = "navigating";

        protected Citation citation;
        protected bool navigating;
        public override async Task LoadQueryInfo(IQueryCollection query)
        {
            navigating = query.ContainsKey(queryKeyNavigating);
            citation = await Task.Run(() =>
            {
                if (!int.TryParse(query[queryKeyStart], out int start)) start = Result.notfound;
                if (!int.TryParse(query[queryKeyEnd], out int end)) end = Result.notfound;
                Citation citation = new Citation(start, end)
                {
                    CitationType = GetCitationType()
                };
                return citation;
            });
            if (citation.CitationRange.WordIndexIsDeferred)
            {
                citation.CitationRange.SetWordIndex(
                    await ReadDeferredWord(query[queryKeyWord].ToString(), 
                    citation.CitationRange.StartID.ID,
                    citation.CitationRange.EndID.ID)
                    );
            }
        }

        //todo find where to put this method
        public static async Task<int> ReadDeferredWord(string indexWord, int start, int end)
        {
            var reader = new DataReaderProvider<string, int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadWordIndex),
                indexWord, start, end);
            return await reader.GetDatum<int>();
        }

        protected abstract CitationTypes GetCitationType();
        public override bool IsValid()
        {
            return (citation != null) && (citation.CitationRange.Valid);
        }

        public override string GetQueryInfo()
        {
            string info = HTMLTags.StartQuery + queryKeyStart + Symbol.equal + citation.CitationRange.StartID +
                HTMLTags.Ampersand + queryKeyEnd + Symbol.equal + citation.CitationRange.EndID;
            return (navigating) ?
                info + HTMLTags.Ampersand + queryKeyNavigating + "=true" :
                info;
        }
    }
}
