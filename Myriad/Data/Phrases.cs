using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.Data;

namespace Myriad.Data
{
    public class Phrases
    {
        internal static List<string> GetPhrases(List<string> words) 
        {
            var result = new List<string>();
            if (words.Count == 1)
            {
                result.Add(words[Ordinals.first]);
                return result;
            }
            for (int index = Ordinals.first; index < words.Count; index++)
            {
                string possibility = GetMatchingPhrase(words, index);
                index += possibility.Count(c => c == ' ');
                result.Add(possibility);
            }
            return result;
        }

        private static string GetMatchingPhrase(List<string> words, int index)
        {
            var possibilities = PhrasesThatStartWith(words[index]);
            string possibility = EvaluatePossibilities(words, index, possibilities);
            return possibility;
        }

        private static string EvaluatePossibilities(List<string> words, int index, List<string> possibilities)
        {
            if (!possibilities.Any()) return words[index];
            for (int i = Ordinals.first; i<possibilities.Count; i++)
            {
                if (PhraseMatches(words, index, possibilities[i]))
                {
                    return possibilities[i];
                }
            }
            return words[index];
        }

        internal static bool PhraseMatches(List<string> words, int index, string testPhrase)
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
                var testWordRoots = Inflections.RootsOf(testWords[testIndex]);
                if (!RootsMatch(testWordRoots, words[index + testIndex])) return false;
                testIndex++;
            }
            return true; 
        }

        private static bool RootsMatch(List<string> testRoots, string word)
        {
            if (testRoots.Contains(word)) return true;
            bool found = false;
            var offsetRoots = Inflections.RootsOf(word).Distinct().ToList();
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

        internal static List<string> RootsOf(string words)
        {
            List<string> result = Inflections.EnglishRootsOf(words);
            if (result.Count > 0) return result;
            StringBuilder phrase = new StringBuilder();
            string[] wordList = words.Split(new char[] { ' ', '_' });
            int count = wordList.Length;
            foreach (string word in wordList)
            {
                var roots = Inflections.EnglishRootsOf(word);
                string root = (roots.Contains(word)) ? word : roots[Ordinals.first];
                phrase.Append(root);
                if (count > 1) phrase.Append(' ');
                count++;
            }
            return new List<string>() { phrase.ToString() };
        }
        internal static List<string> PhrasesThatStartWith(string word)
        {
            var roots = Inflections.HardRootsOf(word);
            var reader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadPhrases),
                word);
            var phrases = reader.GetData<string>();
            for (int i = Ordinals.first; i < roots.Count; i++)
            {
                reader.SetParameter(roots[i]);
                phrases.AddRange(reader.GetData<string>());
            }
            reader.Close();
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
            SearchResult searchword = new SearchResult(searchResult.sentenceID, searchResult.wordIndex, phrase.Replace(' ', '_'),
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
