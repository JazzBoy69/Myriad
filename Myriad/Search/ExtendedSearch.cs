using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feliciana.Data;
using Feliciana.Library;
using Feliciana.ResponseWriter;
using Feliciana.HTML;
using Myriad.Data;
using Myriad.Library;
using Myriad.Parser;
using Myriad.Formatter;

namespace Myriad.Search
{
    public class ExtendedSearch
    {
        public static async Task<List<(int, int)>> EvaluatePhraseDefinitions(List<List<int>> phraseDefinitions)
        {
            if (phraseDefinitions.Count < 2) return new List<(int, int)>();
            var reader = new DataReaderProvider(SqlServerInfo.CreateCommandFromQuery(
                GenerateCommonRangeQuery(phraseDefinitions)));
            var ranges = await reader.GetData<int, int>();
            reader.Close();
            return ranges.Distinct().ToList();
        }

        private static string GenerateCommonRangeQuery(List<List<int>> phraseDefinitions)
        {
            StringBuilder query = new StringBuilder("select maxstart=(SELECT MAX(x) FROM (VALUES ");
            AppendMaxStart(phraseDefinitions, query);
            AppendMinLast(phraseDefinitions, query);
            AppendJoinStatements(phraseDefinitions, query);
            AppendWhereStatement(phraseDefinitions, query);
            return query.ToString();
        }

        private static void AppendWhereStatement(List<List<int>> phraseDefinitions, StringBuilder query)
        {
            query.Append("where ");
            for (int i = Ordinals.first; i < phraseDefinitions.Count; i++)
            {
                if (i > Ordinals.first) query.Append(" and ");
                if (phraseDefinitions[i].Count > 1)
                {
                    AppendOr(query, i, phraseDefinitions[i]);
                    continue;
                }
                query.Append("ra");
                query.Append(i);
                query.Append(".articleid=");
                query.Append(phraseDefinitions[i][Ordinals.first]);
            }
        }

        private static void AppendJoinStatements(List<List<int>> phraseDefinitions, StringBuilder query)
        {
            for (int i = Ordinals.second; i < phraseDefinitions.Count; i++)
            {
                query.Append("join RelatedArticles as ra");
                query.Append(i);
                query.Append(" on ra0.start <= ra");
                query.Append(i);
                query.Append(".last and ra0.last >= ra");
                query.Append(i);
                query.Append(".start ");
            }
        }

        private static void AppendMinLast(List<List<int>> phraseDefinitions, StringBuilder query)
        {
            for (int i = Ordinals.first; i < phraseDefinitions.Count; i++)
            {
                if (i > Ordinals.first) query.Append(",");
                query.Append("(ra");
                query.Append(i);
                query.Append(".last)");
            }
            query.Append(") AS value(x)) from RelatedArticles as ra0 ");
        }

        private static void AppendMaxStart(List<List<int>> phraseDefinitions, StringBuilder query)
        {
            for (int i = Ordinals.first; i < phraseDefinitions.Count; i++)
            {
                if (i > Ordinals.first) query.Append(",");
                query.Append("(ra");
                query.Append(i);
                query.Append(".start)");
            }
            query.Append(") AS value(x)), minlast=(SELECT MIN(x) FROM (VALUES ");
        }

        private static void AppendOr(StringBuilder query, int index, List<int> ids)
        {
            query.Append("(");
            for (int i = Ordinals.first; i < ids.Count; i++)
            {
                if (i > Ordinals.first) query.Append(" or ");
                query.Append("ra");
                query.Append(index);
                query.Append(".articleid=");
                query.Append(ids[i]);
            }
            query.Append(")");
        }

        internal static List<List<ExtendedSearchRange>> GetResults(List<List<int>> phraseDefinitions, List<(int, int)> ranges)
        {
            var results = new List<List<ExtendedSearchRange>>();
            for (int i = Ordinals.first; i < ranges.Count; i++)
            {
                var definitionSearches = ReadDefinitionSearches(phraseDefinitions, ranges[i]);
                results.Add(SplitRange(ranges[i], definitionSearches));
            }
            return results;
        }

        private static List<ExtendedSearchRange> SplitRange((int, int) range, List<ExtendedSearchArticle> definitionSearches)
        {
            definitionSearches = definitionSearches.OrderBy(ds => ds.Start).ToList();
            int end = Math.Min(range.Item2, range.Item1 + 25);
            int start = range.Item1;
            ExtendedSearchRange searchRange = new ExtendedSearchRange(range.Item1, end);
            var result = new List<ExtendedSearchRange>();
            for (int i = Ordinals.first; i < definitionSearches.Count; i++)
            {
                if (definitionSearches[i].Start < end)
                {
                    searchRange.AddSearchResult(definitionSearches[i]);
                    continue;
                }
                result.Add(searchRange.Copy());
                start = definitionSearches[i].Start - 10;
                end = definitionSearches[i].End + 15;
                searchRange = new ExtendedSearchRange(start, end);
                searchRange.AddSearchResult(definitionSearches[i]);
            }
            result.Add(searchRange);
            return result;
        }

        private static List<ExtendedSearchArticle> ReadDefinitionSearches(List<List<int>> phraseDefinitions, (int, int) range)
        {
            var result = new List<ExtendedSearchArticle>();
            var reader = new DataReaderProvider<int, int, int>(SqlServerInfo.GetCommand(DataOperation.ReadExtendedDefinitionSearch),
                -1, -1, -1);
            for (int i = Ordinals.first; i < phraseDefinitions.Count; i++)
            {
                for (int j = Ordinals.first; j < phraseDefinitions[i].Count; j++)
                {
                    reader.SetParameter(phraseDefinitions[i][j], range.Item1, range.Item2);
                    ExtendedSearchArticle article = reader.GetClassDatum<ExtendedSearchArticle>();
                    if (article != null) result.Add(article);
                }
            }
            reader.Close();
            return result;
        }

        internal static async Task WriteResults(HTMLWriter writer, List<List<ExtendedSearchRange>> results)
        {
            for (int i = Ordinals.first; i < results.Count; i++)
            {
                Citation citation = new Citation(results[i][Ordinals.first].Start, results[i][^1].End);
                await writer.Append(HTMLTags.StartParagraph);
                citation.CitationType = CitationTypes.Text;
                await CitationConverter.AppendLink(writer, citation);
                await writer.Append(": ");
                for (int j = Ordinals.first; j < results[i].Count; j++)
                {
                    if (j > Ordinals.first) await writer.Append(HTMLTags.Ellipsis);
                    await AppendResult(writer, results[i][j]);
                }
                await writer.Append(HTMLTags.EndParagraph);
            }
        }

        private static async Task AppendResult(HTMLWriter writer, ExtendedSearchRange extendedSearchRange)
        {
            List<Keyword> keywords = TextSectionFormatter.ReadKeywords(
                extendedSearchRange.Start, extendedSearchRange.End);
            int articleIndex = Ordinals.first;
            for (int i = Ordinals.first; i < keywords.Count; i++)
            {
                if ((articleIndex < extendedSearchRange.DefinitionSearches.Count) &&
                    (extendedSearchRange.DefinitionSearches[articleIndex].Start == keywords[i].ID))
                {
                    await AppendSearchArticle(writer, extendedSearchRange.DefinitionSearches[articleIndex]);
                }
                await TextFormatter.AppendTextOfKeyword(writer, keywords[i]);
                if ((articleIndex < extendedSearchRange.DefinitionSearches.Count) &&
                    (extendedSearchRange.DefinitionSearches[articleIndex].End == keywords[i].ID))
                {
                    await writer.Append(HTMLTags.EndAnchor);
                    articleIndex++;
                    while ((articleIndex < extendedSearchRange.DefinitionSearches.Count) &&
                        (extendedSearchRange.DefinitionSearches[articleIndex].Start <= keywords[i].ID))
                        articleIndex++;
                }
            }
        }

        private static async Task AppendSearchArticle(HTMLWriter writer, ExtendedSearchArticle article)
        {
            await SearchFormatter.AppendSearchArticle(writer, article.Start, article.End, article.ID);
        }
    }
}
