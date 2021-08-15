using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feliciana.Library;

namespace Myriad.Data
{
    public class Phrases
    {
        internal static async Task<List<string>> GetPhrases(List<string> words) 
        {
            var result = new List<string>();
            if (words.Count == 1)
            {
                result.Add(words[Ordinals.first]);
                return result;
            }
            for (int index = Ordinals.first; index < words.Count; index++)
            {
                string possibility = await GetMatchingPhrase(words, index);
                index += possibility.Count(c => c == ' ');
                result.Add(possibility);
            }
            return result;
        }

        private static async Task<string> GetMatchingPhrase(List<string> words, int index)
        {
            var possibilities = PhrasesThatStartWith(words[index]);
            string possibility = await EvaluatePossibilities(words, index, possibilities);
            return possibility;
        }

        private static async Task<string> EvaluatePossibilities(List<string> words, int index, List<string> possibilities)
        {
            if (!possibilities.Any()) return words[index];
            for (int i = Ordinals.first; i<possibilities.Count; i++)
            {
                if (await PhraseMatches(words, index, possibilities[i]))
                {
                    return possibilities[i];
                }
            }
            return words[index];
        }

        internal static async Task<bool> PhraseMatches(List<string> words, int index, string testPhrase)
        {
            string[] testWords = testPhrase.Split(new char[] { ' ' });
            if ((index + testWords.Length) > words.Count) return false;
            int testIndex = Ordinals.first;
            while (testIndex < testWords.Length)
            {
                if (words[testIndex] != testWords[testIndex]) break;
                testIndex++;
            }
            if (testIndex >= testWords.Length) return true;
            testIndex = Ordinals.first;
            while (testIndex < testWords.Length)
            {
                var testWordRoots = await Inflections.RootsOf(testWords[testIndex]);
                if (!await RootsMatch(testWordRoots, words[index + testIndex])) return false;
                testIndex++;
            }
            return true; 
        }

        private static async Task<bool> RootsMatch(List<string> testRoots, string word)
        {
            if (testRoots.Contains(word)) return true;
            bool found = false;
            var offsetRoots = (await Inflections.RootsOf(word)).Distinct().ToList();
            for (int i = Ordinals.first; i<offsetRoots.Count; i++)
            {
                if (testRoots.Contains(offsetRoots[i]))
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        internal static async Task<List<string>> RootsOf(string word)
        {
            List<string> result = await DataRepository.RootsOf(word);
            if (result.Count > 0) return result;
            result = await Inflections.EnglishRootsOf(word);
            if (result.Count > 0) return result;
            StringBuilder phrase = new StringBuilder();
            string[] wordList = word.Split(new char[] { ' ', '_' });
            for (int i=Ordinals.first; i<wordList.Length; i++)
            {
                if (i > Ordinals.first) phrase.Append(' ');
                var roots = await DataRepository.RootsOf(wordList[i]);
                string root = (roots.Contains(wordList[i])) ? wordList[i] : roots[Ordinals.first];
                phrase.Append(root);
            }
            return new List<string>() { phrase.ToString() };
        }
        internal static async Task<List<string>> PhrasesThatStartWith(string word)
        {
            var roots = await Inflections.HardRootsOf(word);
            var phrases = await DataRepository.PhrasesThatStartWith(word);
            for (int i = Ordinals.first; i < roots.Count; i++)
            {
                phrases.AddRange(await DataRepository.PhrasesThatStartWith(roots[i]));
            }
            return phrases.Distinct().ToList();
        }

        internal static async Task Add(string firstWord, string phrase)
        {
            var reader = new DataReaderProvider<string>(SqlServerInfo.GetCommand(DataOperation.ReadPhrase),
                phrase);
            int id = await reader.GetDatum<int>();
            reader.Close();
            if (id > Number.nothing) return;
            await DataWriterProvider.Write<string, string, int>(SqlServerInfo.GetCommand(DataOperation.CreatePhrase),
                firstWord, phrase, phrase.Count(c => c==' ')+1);
            await CreateSearchWordPhrase(phrase);
        }

        private static async Task CreateSearchWordPhrase(string phrase)
        {
            List<string> words = phrase.Split(Symbols.spaceArray, StringSplitOptions.RemoveEmptyEntries).ToList();
            int length = words.Count - 1;
            StringBuilder query = PhraseQuery(words);
            var reader = new DataReaderProvider(SqlServerInfo.CreateCommandFromQuery(query.ToString()));
            List<(int start, int sentenceID, int wordIndex)> searchResults = await reader.GetData<int, int, int>();
            for (int i = Ordinals.first; i < searchResults.Count; i++)
            {
                await AddSearchPhrase(phrase, searchResults[i], length);
            }
        }

        private static StringBuilder PhraseQuery(List<string> words)
        {
            StringBuilder query = new StringBuilder(
            @"select sw0.start, sw0.sentence, sw0.wordindex from searchwords as sw0");
            for (int i = Ordinals.second; i < words.Count; i++)
            {
                query.Append(" join searchwords as sw");
                query.Append(i);
                query.Append(" on sw0.start+");
                query.Append(i);
                query.Append("= sw");
                query.Append(i);
                query.Append(".start");
            }
            query.Append(" where ");
            for (int i = Ordinals.first; i < words.Count; i++)
            {
                if (i > Ordinals.first) query.Append(" and ");
                query.Append("sw");
                query.Append(i);
                query.Append(".text='");
                query.Append(words[i]);
                query.Append("'");
            }

            return query;
        }

        private static async Task AddSearchPhrase(string phrase, (int start, int sentenceID, int wordIndex) searchResult, int length)
        {
            SearchResult searchword = new SearchResult(searchResult.sentenceID, searchResult.wordIndex, phrase,
                0, 500, searchResult.start, searchResult.start + length, 0);
            await DataWriterProvider.WriteDataObject(SqlServerInfo.GetCommand(DataOperation.CreateMatrixWord),
                searchword);
        }
        internal static async Task Add(string phrase)
        {
            int p = phrase.IndexOf(' ');
            if (p == Result.notfound) return;
            string firstWord = phrase.Substring(Ordinals.first, p);
            await Add(firstWord, phrase);
        }
    }
}
