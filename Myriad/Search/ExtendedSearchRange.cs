using Myriad.Data;
using System;
using System.Collections.Generic;

namespace Myriad.Search
{
    internal class ExtendedSearchRange
    {
        readonly int start;
        readonly int end;
        readonly List<ExtendedSearchArticle> definitionSearches = new List<ExtendedSearchArticle>();
        internal int Start => start;
        internal int End => end;

        internal List<ExtendedSearchArticle> DefinitionSearches => definitionSearches;
        internal ExtendedSearchRange(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        internal void AddSearchResult(ExtendedSearchArticle extendedSearchArticle)
        {
            definitionSearches.Add(extendedSearchArticle);
        }

        internal ExtendedSearchRange Copy()
        {
            ExtendedSearchRange newRange = new ExtendedSearchRange(start, end);
            newRange.definitionSearches.AddRange(definitionSearches);
            return newRange;
        }
    }
}