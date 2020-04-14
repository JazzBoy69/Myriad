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
        //todo refactor this class
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
                var roots = await Inflections.HardRootsOf(words[index]);
                foreach (string root in roots)
                {
                    var possibilities = (from phrase in await PhrasesThatStartWith(words[index], root)
                                         orderby phrase.Count(c => c == ' ') descending
                                         select phrase).ToList();
                    bool found = false;
                    if (possibilities.Any())
                    {
                        foreach (string possibility in possibilities)
                        {
                            if (await Compare(words, index, possibility))
                            {
                                result.Add(possibility);
                                index += possibility.Count(c => c == ' ');
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found) result.Add(words[index]);
                }
            }
            return result;
        }

        internal static async Task<bool> Compare(List<string> roots, int index, string testPhrase)
        {
            string[] testWords = testPhrase.Split(new char[] { ' ' });
            if ((index + testWords.Length) > roots.Count) return false;
            int testIndex = Ordinals.first;
            while (testIndex < testWords.Length)
            {
                if (roots[testIndex] != testWords[testIndex]) break;
                testIndex++;
            }
            if (testIndex >= testWords.Length) return true;
            testIndex = Ordinals.first;
            while (testIndex < testWords.Length)
            {
                var testRoots = await Inflections.RootsOf(testWords[testIndex]);
                if (!testRoots.Contains(roots[index + testIndex]))
                {
                    bool found = false;

                    foreach (string root in testRoots.Distinct())
                    {
                        var rootsToTry = await Inflections.RootsOf(testWords[testIndex]);
                        if (rootsToTry.Contains(root))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) return false;
                }
                testIndex++;
            }
            return true;
        }

        internal static async Task<List<string>> RootsOf(string words)
        {
            List<string> result = await Inflections.EnglishRootsOf(words);
            if (result.Count > 0) return result;
            StringBuilder phrase = new StringBuilder();
            string[] wordList = words.Split(new char[] { ' ', '_' });
            int count = wordList.Length;
            foreach (string word in wordList)
            {
                var roots = await Inflections.EnglishRootsOf(word);
                string root = (roots.Contains(word)) ? word : roots[Ordinals.first];
                phrase.Append(root);
                if (count > 1) phrase.Append(' ');
                count++;
            }
            return new List<string>() { phrase.ToString() };
        }
        internal static async Task<List<string>> PhrasesThatStartWith(string word, string root)
        {
            var reader = new DataReaderProvider<string, 
                string>(SqlServerInfo.GetCommand(DataOperation.ReadPhrases),
                word, root);
            return await reader.GetData<string>();
        }
    }
}
