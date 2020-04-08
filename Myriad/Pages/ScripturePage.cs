﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FelicianaLibrary;
using Myriad.Library;
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
            if (!int.TryParse(query[queryKeyStart], out int start)) start = Result.notfound;
            if (!int.TryParse(query[queryKeyEnd], out int end)) end = Result.notfound;
            citation = new Citation(start, end)
            {
                CitationType = GetCitationType()
            };
        }
        protected abstract CitationTypes GetCitationType();

        public abstract void SetupNextPage();
        public abstract void SetupPrecedingPage();
        public override bool IsValid()
        {
            return (citation != null) && (citation.CitationRange.Valid);
        }
    }
}
