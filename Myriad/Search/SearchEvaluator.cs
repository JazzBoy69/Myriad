﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feliciana.Library;
using Myriad.Library;
using Myriad.Data;
using Feliciana.Data;

namespace Myriad.Search
{
    public class SearchEvaluator
    {
        List<int> usedDefinitions = new List<int>();
        List<List<string>> synonyms = new List<List<string>>();
        private const string definitionSelector = @"select sentence, wordindex, id from definitionsearch
            where ";

        public List<int> UsedDefinitions => usedDefinitions;

        internal async Task<List<SearchSentence>> Search(List<string> phrases, 
            CitationRange citationRange)
        {
            var commonWords = new List<string>();
            Dictionary<int, int> sentences = null;
            var searchResults = new List<SearchResult>();
            int queryIndex = Ordinals.first;

            //TODO: Add search for this definition filter
            // var ids = (from idstring in idStrings
            // select Numbers.Convert(idstring)).ToList();
            string rangeSelection = RangeSelection(citationRange);
            bool needSynonymQuery = false;
            for (int index = Ordinals.first; index < phrases.Count; index++)
            {
                if (EnglishDictionary.IsCommonWord(phrases[index]))
                {
                    string root = EnglishDictionary.CommonInflection(phrases[index]);
                    commonWords.Add(root);
                }
                else
                {
                    List<SearchResult> results =
                        await ReadPhraseResults(phrases[index], queryIndex, rangeSelection);
                    searchResults.AddRange(results);
                    var phraseSentences = AddResultsToSentences(sentences, results, 1);
                    Dictionary<int, int> synSentences = null;
                    if (needSynonymQuery)
                    {
                        if (synonyms[queryIndex].Count > 0)
                        {
                            var synResults =
                                await ReadPhrasesResults(queryIndex, synonyms, rangeSelection);
                            searchResults.AddRange(synResults);
                            synSentences = AddResultsToSentences(sentences, synResults, 3);
                        }
                    }
                    sentences = phraseSentences;
                    if (synSentences != null)
                    {
                        foreach (var sentence in synSentences)
                        {
                            if (!sentences.ContainsKey(sentence.Key))
                                sentences.Add(sentence.Key, sentence.Value);
                        }
                    }
                    queryIndex++;
                }
            }
            var filteredResults = (from sentence in sentences
                                   join result in searchResults
                                   on sentence.Key equals result.SentenceID
                                   orderby sentence.Key
                                   select result).ToList();

            //2) Load Definition searches
            var definitionQuery = new StringBuilder(definitionSelector);
            AppendORSelection(definitionQuery, usedDefinitions.Distinct().ToList(), "id =");
            definitionQuery.Append(rangeSelection);
            Dictionary<(int, int), int> definitionSearchesInSentences =
                await ReadDefinitionSearches(definitionQuery.ToString());

            List<SearchSentence> filteredOrSentences = 
                await SetScores(commonWords, sentences, 
                queryIndex, needSynonymQuery, filteredResults, 
                definitionSearchesInSentences);
            return filteredOrSentences ?? new List<SearchSentence>();
        }

        private static async Task<List<SearchSentence>> SetScores(List<string> commonWords, Dictionary<int, int> sentences, int queryIndex, bool needSynonymQuery, List<SearchResult> filteredResults, Dictionary<(int, int), int> definitionSearchesInSentences)
        {
            int lastSentence = -1;
            SearchSentence currentSentence = null;
            var orSentences = new List<SearchSentence>();
            var commonWordCommand = CreateCommonWordQuery(commonWords);

            foreach (SearchResult result in filteredResults)
            {
                if ((definitionSearchesInSentences != null) &&
                    (definitionSearchesInSentences.ContainsKey(result.Key)))
                    result.SetArticleID(definitionSearchesInSentences[result.Key]);
                if (result.SentenceID != lastSentence)
                {
                    lastSentence = result.SentenceID;
                    if (currentSentence != null)
                    {
                        int score = CalculateDistance(currentSentence);
                        currentSentence.SetScore(score);
                        var commonWordIndices = await CommonWordIndices(currentSentence, commonWordCommand);
                        if (commonWordIndices.Count > Number.nothing)
                        {
                            foreach (int index in commonWordIndices)
                            {
                                currentSentence.Add(new SearchResult(currentSentence.SentenceID, index, 1, 0));
                            }
                            currentSentence.SetType(currentSentence.Type-1);
                        }
                        orSentences.Add(currentSentence);
                    }
                    currentSentence = new SearchSentence(
                        result.SentenceID,
                        queryIndex,
                        sentences[result.SentenceID]);
                }
                currentSentence.Add(result);
            }
            List<SearchSentence> filteredOrSentences = null;
            if (currentSentence != null)
            {
                int score = CalculateDistance(currentSentence);
                currentSentence.SetScore(score);
                var commonWordIndices = await CommonWordIndices(currentSentence, commonWordCommand);
                if (commonWordIndices.Count > Number.nothing)
                {
                    foreach (int index in commonWordIndices)
                    {
                        currentSentence.Add(new SearchResult(currentSentence.SentenceID, index, 1, 0));
                    }
                    currentSentence.SetType(currentSentence.Type - 1);
                }
                orSentences.Add(currentSentence);
                filteredOrSentences = (needSynonymQuery) ?
                    (from sentence in orSentences
                     where sentence.Score < 25 && sentence.Type > 1
                     orderby sentence.Type, sentence.Score
                     select sentence).ToList() :
                    (from sentence in orSentences
                     where sentence.Score < 25
                     orderby sentence.Type, sentence.Score
                     select sentence).ToList();
            }
            return filteredOrSentences;
        }

        internal static int CalculateDistance(SearchSentence sentence)
        {
            if (sentence.WordCount == 1) //todo weight single word searches
            {
                return 0;
            }

            List<WordPosition> wordPositions = sentence.WordPositions;
            wordPositions.Sort();
            List<int> wordCounts = sentence.WordCounts;
            int extra = wordPositions.Count - sentence.WordCount;
            //Find smallest union of search phrases
            if (extra > 0)
            {
                WordPosition first = wordPositions.First();
                WordPosition last = wordPositions.Last();
                while ((wordPositions.Count > sentence.WordCount) && ((wordCounts[first.QueryIndex] > 1) ||
                    (wordCounts[last.QueryIndex] > 1)))
                {
                    int low = 1000000;
                    int high = 1000000;
                    if (wordCounts[first.QueryIndex] > 1)
                    {
                        low = last.WordIndex - wordPositions.ElementAt(Ordinals.second).WordIndex;
                    }
                    if (wordCounts[last.QueryIndex] > 1)
                    {
                        high = wordPositions.ElementAt(wordPositions.Count - 2).WordIndex - first.WordIndex;
                    }
                    if (low < high)
                    {
                        wordCounts[first.QueryIndex]--;
                        wordPositions.RemoveAt(Ordinals.first);
                        first = wordPositions.First();
                    }
                    else
                    {
                        wordCounts[last.QueryIndex]--;
                        wordPositions.RemoveAt(wordPositions.Count - 1);
                        last = wordPositions.Last();
                    }
                }
            }
            int coverage = 0;
            foreach (WordPosition entry in wordPositions) coverage += entry.Length;
            int space = (wordPositions.Last().WordIndex - wordPositions.First().WordIndex + 3) - coverage;
            if ((space < 0) || (space > 7)) space = 300;
            //test how ordered the results are
            int disorder = EditDistance(wordPositions, sentence.WordCount);
            return space + disorder;
        }

        private static int EditDistance(List<WordPosition> wordPositions, int wordCount)
        {
            int n = wordPositions.Count;
            int m = wordCount;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = ((j - 1) == wordPositions.ElementAt(i - 1).QueryIndex) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m] - (wordPositions.Count - wordCount);
        }


        private static string RangeSelection(CitationRange searchRange)
        {
            return ((searchRange == null) || (!searchRange.Valid)) ? "" :
                String.Concat(" and (start>=", searchRange.StartID,
                " and last<=", searchRange.EndID, ") ");
        }

        private static async Task<List<SearchResult>> ReadPhraseResults(string phrase, int queryIndex
            , string rangeSelection)
        {
            var orQuery = new StringBuilder(
                "select sw.sentence, sw.wordindex, sw.last-sw.start+1, ");
            orQuery.Append(queryIndex);
            orQuery.Append(" from searchwords as sw where ");
            orQuery.Append("sw.text='");
            orQuery.Append(phrase.Replace(' ', '_'));
            orQuery.Append("'");
            orQuery.Append(rangeSelection);
            var results = await ReadSearchResults(orQuery.ToString());
            return results;
        }

        private static async Task<List<SearchResult>> ReadSearchResults(string query)
        {
            var reader = new DataReaderProvider(SqlServerInfo.CreateCommandFromQuery(query));
            var result = await reader.GetClassData<SearchResult>();
            reader.Close();
            return result;
        }

        private static async Task<List<SearchResult>> ReadPhrasesResults(int queryIndex,
                List<List<string>> synonyms, string rangeSelection)
        {
            var synQuery = new StringBuilder(
            "select sw.sentence, sw.wordindex, sw.last-sw.start+1, ");
            synQuery.Append(queryIndex);
            synQuery.Append(" from searchwords as sw where ");
            AppendORSelection(synQuery, synonyms[queryIndex], "sw.text");
            synQuery.Append(rangeSelection);
            return await ReadSearchResults(synQuery.ToString());
        }
        private static void AppendORSelection(StringBuilder builder, List<int> list,
            string selector)
        {
            builder.Append("(");
            for (int i = Ordinals.first; i < list.Count; i++)
            {
                if (i > Ordinals.first) builder.Append(" or ");
                builder.Append(selector);
                builder.Append(list[i]);
            }
            builder.Append(") ");
        }
        private static void AppendORSelection(StringBuilder builder, List<string> list,
            string selector)
        {
            builder.Append(selector);
            builder.Append(" in (");
            for (int i = Ordinals.first; i < list.Count; i++)
            {
                if (i > Ordinals.first) builder.Append(", ");
                builder.Append("'");
                builder.Append(list[i].Replace(' ', '_'));
                builder.Append("'");
            }
            builder.Append(") ");
        }

        internal async Task EvaluateSynonyms(List<string> phrases)
        {
            foreach (string phrase in phrases)
            {
                if (EnglishDictionary.IsCommonWord(phrase)) continue;
                var roots = await Inflections.HardRootsOf(phrase.Replace('_', ' '));
                //get definition ids for phrase
                List<int> definitionIDs = await GetDefinitionIDs(roots);
                //get synonyms for phrase
                var thisPhraseSynonyms = await GetSynonyms(definitionIDs, phrase);
                synonyms.Add(thisPhraseSynonyms);

                usedDefinitions.AddRange(definitionIDs);
            }
        }

        private static async Task<List<string>> GetSynonyms(List<int> definitionIDs, string root)
        {
            var reader = new DataReaderProvider<int, string>(SqlServerInfo.GetCommand(DataOperation.ReadSynonyms),
                -1, "");
            var results = new List<string>();
            for (int index = Ordinals.first; index < definitionIDs.Count; index++)
            {
                reader.SetParameter(definitionIDs[index], root);
                results.AddRange(await reader.GetData<string>());
            }
            return results;
        }

        private static async Task<List<int>> GetDefinitionIDs(List<string> roots)
        {
            var results = new List<int>();
            var reader = new DataReaderProvider<string>(SqlServerInfo.GetCommand(
                DataOperation.ReadDefinitionIDs),
                "");
            for (int index = Ordinals.first; index < roots.Count; index++)
            {
                reader.SetParameter(roots[index]);
                results.AddRange(await reader.GetData<int>());
            }
            reader.Close();
            return results;
        }

        private static Dictionary<int, int> AddResultsToSentences(Dictionary<int, int> sentences, List<SearchResult> results, int type)
        {
            Dictionary<int, int> newSentences = (sentences == null) ?
                results.Select(result => result.SentenceID).Distinct()
                .ToDictionary(
                    r => r,
                    r => type) :
                (from result in results
                 join sentence in sentences
                         on result.SentenceID equals sentence.Key
                 select sentence).Distinct().ToDictionary(
                                s => s.Key,
                                s => Math.Max(s.Value, type));
            return newSentences;
        }
        private static async Task<Dictionary<(int, int), int>> ReadDefinitionSearches(string query)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.CreateCommandFromQuery(query), -1);
            List<(int sentence, int wordindex, int id)> preliminaryResults = 
                await reader.GetData<int, int, int>();
            reader.Close();
            var results = new Dictionary<(int, int), int>();
            for (int index = Ordinals.first; index < preliminaryResults.Count; index++)
            {
                (int, int) key = (preliminaryResults[index].sentence, 
                    preliminaryResults[index].wordindex);
                if (results.ContainsKey(key)) continue;
                results.Add(key, preliminaryResults[index].id);
            }
            return results;
        }
        private static async Task<List<int>> CommonWordIndices(SearchSentence currentSentence,
            string commonWordQuery)
        {
            if (string.IsNullOrEmpty(commonWordQuery)) return new List<int>();
            if (currentSentence.Score > 25) return new List<int>();
            return await LookupCommonWords(commonWordQuery, currentSentence);
        }

        private static string CreateCommonWordQuery(List<string> commonWords)
        {
            if (commonWords.Count == Number.nothing) return null;
            var queryBuilder = new StringBuilder(@"select wordindex from searchwords where sentence=@key1
and wordindex>@key2 and wordindex<@key3 and ");
            AppendORSelection(queryBuilder, commonWords, "text");
            return queryBuilder.ToString();
        }

        private static async Task<List<int>> LookupCommonWords(string query, SearchSentence sentence)
        {
            try
            {
                var reader = new DataReaderProvider<int, int, int>(
                    SqlServerInfo.CreateCommandFromQuery(query),
                    sentence.SentenceID,
                    sentence.FirstPosition,
                    sentence.LastPosition);
                return await reader.GetData<int>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new List<int>();
            }
        }

    }
}