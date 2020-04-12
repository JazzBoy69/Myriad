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
    public abstract class ScripturePage : CommonPage
    {
        internal const string queryKeyTGStart = "tgstart=";
        internal const string queryKeyTGEnd = "tgend=";
        public const string queryKeyStart = "start=";
        public const string queryKeyEnd = "end=";

        protected Citation citation;
        public override async Task LoadQueryInfo(IQueryCollection query)
        {
            citation = await Task.Run(() =>
            {
                if (!int.TryParse(query["start"], out int start)) start = Result.notfound;
                if (!int.TryParse(query["end"], out int end)) end = Result.notfound;
                citation = new Citation(start, end)
                {
                    CitationType = GetCitationType()
                };
                return citation;
            });
            if (citation.CitationRange.WordIndexIsDeferred)
            {
                citation.CitationRange.SetWordIndex(
                    await ReadDeferredWord(query["word"].ToString())
                    );
            }
        }

        private async Task<int> ReadDeferredWord(string indexWord)
        {
            try
            {
                var reader = new DataReaderProvider<string, int, int>(
                    SqlServerInfo.GetCommand(DataOperation.ReadWordIndex),
                    indexWord, citation.CitationRange.StartID.ID, citation.CitationRange.EndID.ID);
                return await reader.GetDatum<int>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -1;
            }
        }

        protected abstract CitationTypes GetCitationType();

        public abstract Task SetupNextPage();
        public abstract Task SetupPrecedingPage();
        public override bool IsValid()
        {
            return (citation != null) && (citation.CitationRange.Valid);
        }

        public override string GetQueryInfo()
        {
            return HTMLTags.StartQuery + queryKeyStart + citation.CitationRange.StartID +
                HTMLTags.Ampersand + queryKeyEnd + citation.CitationRange.EndID;
        }
    }
}
