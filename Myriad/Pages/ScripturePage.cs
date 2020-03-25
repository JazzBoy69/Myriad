using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        protected KeyID startID;
        protected KeyID endID;
        public override void LoadQueryInfo(IQueryCollection query)
        {
            int start;
            int end;
            if (!int.TryParse(query[queryKeyStart], out start)) start = Result.notfound;
            if (!int.TryParse(query[queryKeyEnd], out end)) end = Result.notfound;
            startID = new KeyID(start);
            endID = new KeyID(end);
        }

        public override bool IsValid()
        {
            return (startID != null) && (endID != null) && (startID.Valid) && (endID.Valid);
        }
    }
}
