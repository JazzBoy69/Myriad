﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;
using Myriad.Parser;
using Microsoft.AspNetCore.Http;

namespace Myriad.Pages
{
    public abstract class ScripturePage : CommonPage
    {
        internal const string queryKeyTGStart = "tgstart=";
        internal const string queryKeyTGEnd = "tgend=";
        public const string queryKeyStart = "start";
        public const string queryKeyEnd = "end";

        protected Citation citation;
        public override void LoadQueryInfo(IQueryCollection query)
        {
            int start;
            int end;
            if (!int.TryParse(query[queryKeyStart], out start)) start = Result.notfound;
            if (!int.TryParse(query[queryKeyEnd], out end)) end = Result.notfound;
            citation = new Citation(start, end);
            citation.CitationType = GetCitationType();
        }
        protected abstract CitationTypes GetCitationType();
        public override bool IsValid()
        {
            return (citation != null) && (citation.CitationRange.Valid);
        }
    }
}