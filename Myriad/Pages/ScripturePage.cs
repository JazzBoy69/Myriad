using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.Data;
using Feliciana.HTML;
using Myriad.Library;
using Myriad.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Text;
using Myriad.Data;

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

        internal Citation citation;
        protected Citation targetCitation;
        protected bool navigating;

        public void SetCitation(Citation citation)
        {
            this.citation = citation;
        }

        public void SetTargetCitation(Citation citation)
        {
            targetCitation = citation;
        }
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
            if (!int.TryParse(query[queryKeyTGStart], out int start)) start = Result.notfound;
            if (!int.TryParse(query[queryKeyTGEnd], out int end)) end = Result.notfound;
            targetCitation = new Citation(start, end);

            if (citation.WordIndexIsDeferred)
            {
                citation.SetWordIndex(await DataRepository.TextWordIndex(citation.Start,
                    citation.End, citation.Word));
            }
        }


        protected abstract CitationTypes GetCitationType();
        public override bool IsValid()
        {
            return (citation != null) && (citation.Valid);
        }

        public override string GetQueryInfo()
        {
            StringBuilder info = new StringBuilder(HTMLTags.StartQuery + queryKeyStart + Symbol.equal);
            info.Append(citation.Start);
            info.Append(HTMLTags.Ampersand + queryKeyEnd + Symbol.equal);
            info.Append(citation.End);
            if ((targetCitation != null) && (targetCitation.Valid))
            {
                info.Append(HTMLTags.Ampersand + queryKeyTGStart + Symbol.equal);
                info.Append(targetCitation.Start);
                info.Append(HTMLTags.Ampersand + queryKeyTGEnd + Symbol.equal);
                info.Append(targetCitation.End);
            }
            return (navigating) ?
                info.Append(HTMLTags.Ampersand + queryKeyNavigating + "=true").ToString() :
                info.ToString(); 
        }
    }
}
