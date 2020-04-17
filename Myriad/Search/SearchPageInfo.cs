using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;
using Myriad.Library;

namespace Myriad.Search
{
    public class SearchPageInfo
    {
        CitationRange citationRange;
        string searchQuery;
        string query;
        string all;
        List<string> idList;
        string ids;
        List<SearchSentence> results;
        List<int> usedDefinitions;

        internal string SearchQuery => searchQuery;
        internal CitationRange CitationRange => citationRange;
        internal string Query => query;
        internal string All => all;
        internal List<string> IDList => idList;
        internal string IDs => ids;
        internal List<SearchSentence> SearchResults => results;
        internal List<int> UsedDefinitions => usedDefinitions;

        internal List<string> QueryWords => query.Split(Symbols.spaceArray,
                StringSplitOptions.RemoveEmptyEntries).ToList();

        internal void SetCitationRange(CitationRange range)
        {
            citationRange = range;
        }

        internal void SetQuery(string query)
        {
            this.query = query;
        }

        internal void SetSearchQuery(string query)
        {
            searchQuery = query;
        }

        internal void SetIDs(string ids)
        {
            this.ids = ids;
            idList = ids.Split(Symbols.spaceArray, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        internal void SetResults(List<SearchSentence> results)
        {
            this.results = results;
        }

        internal void SetUsedDefinitions(List<int> definitions)
        {
            usedDefinitions = definitions.Distinct().ToList();
        }
    }
}
