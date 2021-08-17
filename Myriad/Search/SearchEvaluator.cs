using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feliciana.Library;
using Myriad.Library;
using Myriad.Data;

namespace Myriad.Search
{
    public class SearchEvaluator
    {
        List<int> usedDefinitions = new List<int>();
        List<List<string>> synonyms = new List<List<string>>();
        List<List<int>> phraseDefinitions = new List<List<int>>();
        List<SearchResult> filteredResults;
        List<SearchResult> searchResults;
        Dictionary<int, int> sentences = null;
        List<string> commonWords = new List<string>();
        Dictionary<(int, int), int> definitionSearchesInSentences = new Dictionary<(int, int), int>();
        private const string definitionSelector = @"select sentence, wordindex, id from definitionsearch
            where ";


        internal List<List<int>> PhraseDefinitions => phraseDefinitions;
        public List<int> UsedDefinitions => usedDefinitions;

        internal List<SearchResult> SearchResults => searchResults;

        internal async Task<List<SearchSentence>> Search(List<string> phrases, 
            SearchPageInfo pageInfo, bool isSynonymQuery)
        {
            searchResults = new List<SearchResult>();
            int queryIndex = Ordinals.first;

            for (int index = Ordinals.first; index < phrases.Count; index++)
            {
                if (EnglishDictionary.IsCommonWord(phrases[index]))
                {
                    string root = EnglishDictionary.CommonInflection(phrases[index]);
                    commonWords.Add(root);
                    List<SearchResult> results =
                        ((pageInfo.CitationRange != null) && (pageInfo.CitationRange.Valid))
                        ? await DataRepository.OccurencesOfCommonWord(phrases[index], queryIndex, pageInfo.CitationRange.StartID.ID, pageInfo.CitationRange.EndID.ID)
                        : await DataRepository.OccurencesOfCommonWord(phrases[index], queryIndex);                 
                    if (results.Count > Number.nothing)
                    {
                        searchResults.AddRange(results);
                        sentences = AddResultsToSentences(sentences, results, 1);
                        queryIndex++;
                    }
                }
                else
                {
                    List<SearchResult> results = await 
                        ReadPhraseResults(phrases[index], queryIndex, pageInfo.CitationRange);
                    searchResults.AddRange(results);
                    var phraseSentences = AddResultsToSentences(sentences, results, 1);
                    Dictionary<int, int> synSentences = null;
                    if (isSynonymQuery && (synonyms[queryIndex].Count > 0))
                    {
                        var synResults =
                            ReadPhrasesResults(queryIndex, synonyms, pageInfo.CitationRange);
                        searchResults.AddRange(synResults);
                        synSentences = AddResultsToSentences(sentences, synResults, 3);

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
            //2) Load Definition searches
            if (usedDefinitions.Count > Number.nothing)
            {
                usedDefinitions = usedDefinitions.Distinct().ToList();
                definitionSearchesInSentences =
                    ((pageInfo.CitationRange != null) && (pageInfo.CitationRange.Valid))
                    ? await DefinitionSearches(usedDefinitions, pageInfo.CitationRange.StartID.ID, pageInfo.CitationRange.EndID.ID)
                    : await DefinitionSearches(usedDefinitions);
            }
            IEnumerable<KeyValuePair<int, int>> filteredSentences;
            if (pageInfo.IDList != null)
            {
                var ids = new List<int>();
                for (int i = Ordinals.first; i < pageInfo.IDList.Count; i++)
                {
                    ids.Add(Numbers.Convert(pageInfo.IDList[i]));
                }
                var idSentences = from sr in definitionSearchesInSentences
                                  join id in ids on
                                  sr.Value equals id
                                  select sr.Key.Item1;
                filteredSentences = from sentence in sentences
                                    join idSentence in idSentences
                                    on sentence.Key equals idSentence
                                    select sentence;
            }
            else
                filteredSentences = sentences;

            filteredResults = (from sentence in filteredSentences
                                   join result in searchResults
                                   on sentence.Key equals result.SentenceID
                                   orderby sentence.Key
                                   select result).ToList();
            List<SearchSentence> filteredOrSentences = 
                await SetScores(queryIndex, isSynonymQuery, true) ?? new List<SearchSentence>();
            if (filteredOrSentences.Count<10) filteredOrSentences =
                    await SetScores(queryIndex, isSynonymQuery, false) ?? new List<SearchSentence>();
            if ((!isSynonymQuery) && (phrases.Count > 1))
            {
                var completePhraseSentences = await CompletePhraseSearch(pageInfo.Query, pageInfo.CitationRange);
                for (int index = Ordinals.first; index < completePhraseSentences.Count; index++)
                {
                    if (!sentences.ContainsKey(completePhraseSentences[index].SentenceID))
                        filteredOrSentences.Add(completePhraseSentences[index]);
                }
            }
            int limit = (filteredOrSentences.Count > 50) ? 25 : 100;
            filteredOrSentences = (isSynonymQuery) ?
                (from sentence in filteredOrSentences
                 where sentence.Score < limit && sentence.Type > 1
                 orderby sentence.Type, sentence.Score
                 select sentence).ToList() :
                (from sentence in filteredOrSentences.Distinct()
                 where sentence.Score < limit
                 orderby sentence.Type, sentence.Score
                 select sentence).ToList();
            if (!isSynonymQuery && EnglishDictionary.ContainsCommonWords(pageInfo.QueryWords))
            {
                filteredOrSentences.InsertRange(Ordinals.first, await ExactPhraseSearch(pageInfo, pageInfo.CitationRange));
            }
            return filteredOrSentences;
        }

        internal async Task<List<SearchSentence>> ExactPhraseSearch(SearchPageInfo pageInfo, CitationRange searchRange)
        {
            int length = pageInfo.QueryWords.Count;
            if (length < 2) return new List<SearchSentence>();
            var results = await DataRepository.PhraseSearch(pageInfo.QueryWords);
            List<SearchSentence> sentences = new List<SearchSentence>();
            for (int i = Ordinals.first; i < results.Count; i++)
            {
                SearchSentence sentence = new SearchSentence(results[i].sentenceID, length);
                for (int j = Ordinals.first; j < length; j++)
                {
                    var result = new SearchResult(results[i].sentenceID, results[i].wordIndex+j, 1, j);
                    if ((definitionSearchesInSentences != null) &&
                        (definitionSearchesInSentences.ContainsKey(result.Key)))
                        result.SetArticleID(definitionSearchesInSentences[result.Key]);
                    sentence.Add(result);
                }
                sentence.SetScore(1);
                sentence.SetType(0);
                sentences.Add(sentence);
            } 
            return sentences;
        }

        private async Task<List<SearchSentence>> CompletePhraseSearch(string query, CitationRange searchRange)
        {
            var results = await ReadPhraseResults(query, 0, searchRange);
            int count = query.Count(c => c == ' ');
            List<SearchSentence> sentences = new List<SearchSentence>();
            for (int i=Ordinals.first; i<results.Count; i++)
            {
                if ((definitionSearchesInSentences != null) &&
                    (definitionSearchesInSentences.ContainsKey(results[i].Key)))
                    results[i].SetArticleID(definitionSearchesInSentences[results[i].Key]);
                SearchSentence sentence = new SearchSentence(results[i].SentenceID, count);
                sentence.Add(results[i]);
                sentence.SetScore(1);
                sentence.SetType(0);
                sentences.Add(sentence);
            }
            return sentences;
        }

        private async Task<List<SearchSentence>> SetScores(int wordCount, bool isSynonymQuery, bool filterDistance)
        {
            int lastSentence = -1;
            SearchSentence currentSentence = null;
            var orSentences = new List<SearchSentence>();
            for (int i=Ordinals.first; i<filteredResults.Count; i++)
            {
                if ((definitionSearchesInSentences != null) &&
                    (definitionSearchesInSentences.ContainsKey(filteredResults[i].Key)))
                    filteredResults[i].SetArticleID(definitionSearchesInSentences[filteredResults[i].Key]);
                if (filteredResults[i].SentenceID != lastSentence)
                {
                    lastSentence = filteredResults[i].SentenceID;
                    if (currentSentence != null)
                    {
                        int score = CalculateDistance(currentSentence, filterDistance);
                        currentSentence.SetScore(score);
                        var commonWordIndices = await CommonWordIndices (currentSentence, commonWords);
                        if (commonWordIndices.Count > Number.nothing)
                        {
                            for (int j = Ordinals.first; j < commonWordIndices.Count; j++)
                            {
                                currentSentence.Add(new SearchResult(currentSentence.SentenceID, commonWordIndices[j], 1, 0));
                            }
                            currentSentence.SetType(currentSentence.Type-1);
                        }
                        orSentences.Add(currentSentence);
                    }
                    currentSentence = new SearchSentence(
                        filteredResults[i].SentenceID,
                        wordCount,
                        sentences[filteredResults[i].SentenceID]);
                }
                currentSentence.Add(filteredResults[i]);
            }

            if (currentSentence != null)
            {
                int score = CalculateDistance(currentSentence, filterDistance);
                currentSentence.SetScore(score);
                var commonWordIndices = await CommonWordIndices(currentSentence, commonWords);
                if (commonWordIndices.Count > Number.nothing)
                {
                    for (int j=Ordinals.first; j<commonWordIndices.Count; j++)
                    {
                        currentSentence.Add(new SearchResult(currentSentence.SentenceID, commonWordIndices[j], 1, 0));
                    }
                    currentSentence.SetType(currentSentence.Type - 1);
                }
                orSentences.Add(currentSentence);
            }
            return orSentences;
        }

        internal static int CalculateDistance(SearchSentence sentence, bool filterDistance)
        {
            if (sentence.WordCount == 1)
            {
                int score = 25;
                for (int i = Ordinals.first; i < sentence.Words.Count; i++)
                {
                    if (sentence.Words[i].ArticleID > Number.nothing) score -= 5;
                }
                score -= sentence.Words.Count;
                return score;
            }

            List<WordPosition> wordPositions = sentence.WordPositions;
            int center = sentence.Center;
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
                    int low = -1000000;
                    int high = -1000000;
                    if (wordCounts[first.QueryIndex] > 1)
                    {
                        low = center - wordPositions.ElementAt(Ordinals.first).WordIndex;
                    }
                    if (wordCounts[last.QueryIndex] > 1)
                    {
                        high = wordPositions.ElementAt(wordPositions.Count - 1).WordIndex - center;
                    }
                    if (low > high)
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
            for (int i = Ordinals.first; i < wordPositions.Count; i++) coverage += wordPositions[i].Length;
            int space = (wordPositions.Last().WordIndex - wordPositions.First().WordIndex + 3) - coverage;
            if (filterDistance && ((space < 0) || (space > 7))) space = 300;
            int disorder = EditDistance(wordPositions, sentence.WordCount);
            return space + disorder;
        }

        private static int EditDistance(List<WordPosition> wordPositions, int wordCount)
        {
            int width = wordPositions.Count+1;
            int height = wordCount+1;
            int[,] grid = new int[width, height];
            if (width == 0) return height;
            if (height == 0) return width;

            for (int x = 0; x < width; grid[x, 0] = x++) ;
            for (int y = 0; y < height; grid[0, y] = y++) ;

            for (int x = 1; x < width; x++)
            {
                for (int y = 1; y < height; y++)
                {
                    int cost = ((y - 1) == wordPositions.ElementAt(x - 1).QueryIndex) ? 0 : 1;

                    grid[x, y] = Math.Min(
                        Math.Min(grid[x - 1, y] + 1, grid[x, y - 1] + 1),
                        grid[x - 1, y - 1] + cost);
                }
            }
            return grid[width-1, height-1] - (wordPositions.Count - wordCount);
        }


        private static string RangeSelection(CitationRange searchRange)
        {
            return ((searchRange == null) || (!searchRange.Valid)) ? "" :
                String.Concat(" and (start>=", searchRange.StartID,
                " and last<=", searchRange.EndID, ") ");
        }

        private static async Task<List<SearchResult>> ReadPhraseResults(string phrase, int queryIndex, 
            CitationRange searchRange)
        {
            string query = PhraseQuery(phrase, queryIndex, searchRange);
            var results = ReadSearchResults(query);
            if (!phrase.Contains(' ')) return results;
            query = await PhraseWordQuery(phrase, queryIndex, searchRange);
            if (string.IsNullOrEmpty(query)) return results;
            results.AddRange(ReadSearchResults(query));
            return results;
        }

        private static async Task<string> PhraseWordQuery(string phrase, int queryIndex, CitationRange searchRange)
        {
            var words = phrase.Split(Symbols.spaceArray, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (int i = Ordinals.first; i < words.Count; i++)
            {
                if (EnglishDictionary.IsCommonWord(words[i]))
                    return "";
            }
            var query = new StringBuilder(
                "select sw0.sentence, sw0.wordindex, ");
            query.Append(words.Count);
            query.Append(", ");
            query.Append(queryIndex);
            query.Append(", sw0.substitute, RTrim(sw0.text) from searchwords as sw0 ");
            for (int i = Ordinals.second; i < words.Count; i++)
            {
                query.Append("join searchwords as sw");
                query.Append(i);
                query.Append(" on sw");
                query.Append(i);
                query.Append(".start = sw0.start+");
                query.Append(i);
                query.Append(" ");
            }
            query.Append("where ");
            for (int i = Ordinals.first; i < words.Count; i++)
            {
                if (i > Ordinals.first) query.Append(" and ");
                var variations = await Variations(words[i]);
                AppendORSelection(query, variations, "sw" + i + ".text");
            }
            if ((searchRange != null) && (searchRange.Valid))
            {
                query.Append(" and (sw0.start>=");
                query.Append(searchRange.StartID);
                query.Append(" and sw0.last<=");
                query.Append(searchRange.EndID);
                query.Append(") ");
            }
            return query.ToString();
        }

        private static async Task<List<string>> Variations(string word)
        {
            var result = new List<string>() { word };
            result.AddRange(await Inflections.RootsOf(word));
            return result.Distinct().ToList();
        }
        private static string PhraseQuery(string phrase, int queryIndex, CitationRange searchRange)
        {
            var query = new StringBuilder(
                            "select sw.sentence, sw.wordindex, sw.last-sw.start+1, ");
            query.Append(queryIndex);
            query.Append(", sw.substitute, RTrim(sw.text) from searchwords as sw where sw.text='");
            query.Append(phrase);
            query.Append("'");
            if ((searchRange != null) && (searchRange.Valid))
            {
                query.Append(" and (sw.start>=");
                query.Append(searchRange.StartID);
                query.Append(" and sw.last<=");
                query.Append(searchRange.EndID);
                query.Append(") ");
            }
            return query.ToString();
        }

        private static List<SearchResult> ReadPhrasesResults(int queryIndex,
                List<List<string>> synonyms, CitationRange searchRange)
        {
            var synQuery = new StringBuilder(
            "select sw.sentence, sw.wordindex, sw.last-sw.start+1, ");
            synQuery.Append(queryIndex);
            synQuery.Append(", sw.substitute, RTrim(sw.text) from searchwords as sw where ");
            AppendORSelection(synQuery, synonyms[queryIndex], "sw.text");
            if ((searchRange != null) && (searchRange.Valid))
            {
                synQuery.Append(" and (sw.start>=");
                synQuery.Append(searchRange.StartID);
                synQuery.Append(" and sw.last<=");
                synQuery.Append(searchRange.EndID);
                synQuery.Append(") ");
            }
            return ReadSearchResults(synQuery.ToString());
        }

        private static void AppendORSelection(StringBuilder builder, List<string> list,
            string selector)
        {
            if (list.Count == 0) return;
            if (list.Count == 1)
            {
                builder.Append(selector);
                builder.Append("='");
                builder.Append(list[Ordinals.first]);
                builder.Append("'");
                return;
            }
            builder.Append(selector);
            builder.Append(" in (");
            for (int i = Ordinals.first; i < list.Count; i++)
            {
                if (i > Ordinals.first) builder.Append(", ");
                builder.Append("'");
                builder.Append(list[i]);
                builder.Append("'");
            }
            builder.Append(") ");
        }

        internal async Task EvaluateSynonyms(List<string> phrases)
        {
            for (int i=Ordinals.first; i<phrases.Count; i++)
            {
                if (EnglishDictionary.IsCommonWord(phrases[i])) continue;
                var roots = await Inflections.HardRootsOf(phrases[i]);
                //get definition ids for phrase
                for (int j = Ordinals.first; j < roots.Count; j++)
                {
                    var definitionIDs = await DataRepository.ArticleIDsFromSynonym(roots[j]);
                    phraseDefinitions.Add(definitionIDs);
                    //get synonyms for phrase
                    var thisPhraseSynonyms = (definitionIDs.Count == 0) ?
                        roots :
                        await GetSynonyms(definitionIDs, phrases[i]);
                    synonyms.Add(thisPhraseSynonyms);
                    usedDefinitions.AddRange(definitionIDs);
                }
            }
        }

        public static async Task<List<string>> GetSynonyms(List<int> definitionIDs, string root)
        {
            var results = new List<string>();
            for (int index = Ordinals.first; index < definitionIDs.Count; index++)
            {
                results.AddRange(await DataRepository.SynonymsExcept(definitionIDs[index], root));
            }
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
        private static async Task<Dictionary<(int, int), int>> DefinitionSearches(List<int> ids, int start, int last)
        {
            List<(int sentence, int wordindex, int id)> preliminaryResults = new List<(int sentence, int wordindex, int id)>();
            for (int index = Ordinals.first; index < ids.Count; index++)
            {
                preliminaryResults.AddRange(
                    await DataRepository.SentencesWithDefinitionSearch(ids[index], start, last));
            }
            return DistinctSentences(preliminaryResults);
        }
        private static async Task<Dictionary<(int, int), int>> DefinitionSearches(List<int> ids)
        {
            List<(int sentence, int wordindex, int id)> preliminaryResults = new List<(int sentence, int wordindex, int id)>();
            for (int index = Ordinals.first; index < ids.Count; index++)
            {
                preliminaryResults.AddRange(
                    await DataRepository.SentencesWithDefinitionSearch(ids[index]));
            }
            return DistinctSentences(preliminaryResults);
        }
        private static Dictionary<(int, int), int> DistinctSentences(List<(int sentence, int wordindex, int id)> sentences)
        {
            var results = new Dictionary<(int, int), int>();
            for (int index = Ordinals.first; index < sentences.Count; index++)
            {
                (int, int) key = (sentences[index].sentence,
                    sentences[index].wordindex);
                if (results.ContainsKey(key)) continue;
                results.Add(key, sentences[index].id);
            }
            return results;
        }
        private static async Task<List<int>> CommonWordIndices(SearchSentence currentSentence,
            List<string> commonWords)
        {
            if ((commonWords == null) || (commonWords.Count == 0)) return new List<int>();
            if (currentSentence.Score > 25) return new List<int>();
            return await DataRepository.CommonWordsInSentence(commonWords, currentSentence);
        }
    }
}
