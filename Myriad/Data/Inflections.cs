using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.Data;

namespace Myriad.Data
{
    public class Inflections
    {
        //todo refactor this class
        internal static async Task<List<string>> RootsOf(string word)
        {
            if (word.Contains(' ')) return await Phrases.RootsOf(word);
            int index = word.IndexOf('`');
            if (index == Result.notfound)
            {
                index = word.IndexOf('\'');
            }
            word = word.Replace("'", "’").Replace("’s", "").Replace("’", "");
            if (index == Result.notfound)
            {
                if (RemoveNameDiacritics(word) != word) return new List<string>() { RemoveNameDiacritics(word) };
                List<string> roots = await EnglishRootsOf(word);
                if (roots.Count > 0) return roots;
                if (EnglishDictionary.IsCommonWord(word))
                {
                    if (EnglishDictionary.CommonInflection(word) != word) return new List<string>()
                    { EnglishDictionary.CommonInflection(word) };
                }
                return new List<string>() { };
            }
            return new List<string>() { };
        }
        internal static async Task<List<string>> HardRootsOf(string word)
        {
            if (word.Contains(' ')) return await Phrases.RootsOf(word);
            int index = word.IndexOf('`');
            if (index == Result.notfound)
            {
                index = word.IndexOf('\'');
            }
            word = word.Replace("'", "’").Replace("’s", "").Replace("’", "");
            if (index == Result.notfound)
            {
                if (RemoveNameDiacritics(word) != word) return new List<string>() { RemoveNameDiacritics(word) };
                List<string> roots = await EnglishRootsOf(word);
                if (roots.Count > 0) return roots;
                if (EnglishDictionary.IsCommonWord(word))
                {
                    if (EnglishDictionary.CommonInflection(word) != word) return new List<string>()
                    { EnglishDictionary.CommonInflection(word) };
                }
                return new List<string>() { word };
            }
            word = word.Substring(0, index);
            return new List<string>() { word };

        }

        internal static async Task<List<string>> EnglishRootsOf(string word)
        {
            word = word.Replace("'", "’").Replace("’s", "").Replace("’", "");
            if ((word.Contains('_')) || (word.Contains(' ')))
            {
                return await EnglishRootsOfPhrase(word);
            }
            var reader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadRoots),
                word);
            var results = await reader.GetData<string>();
            reader.Close();
            return results;
        }

        private static async Task<List<string>> EnglishRootsOfPhrase(string phrase)
        {
            var phraseReader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadRoots),
                phrase.Replace('_', ' '));
            List<string> result = await phraseReader.GetData<string>();
            phraseReader.Close();
            if (result.Count > 0) return result;
            string[] words = phrase.Split(new char[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string phraseResult = "";
            for (int index = Ordinals.first; index < words.Length; index++)
            {
                if (index > Ordinals.first) phraseResult += ' ';
                List<string> roots = await EnglishRootsOf(words[index]);
                phraseResult += (roots.Count == 0 || roots.Contains(words[index])) ?
                    words[index] :
                    roots.First();
            }
            if (!string.IsNullOrEmpty(phraseResult))
            {
                result.Add(phraseResult);
            }
            return result;
        }

        internal static string RemoveNameDiacritics(string word)
        {
            string result = word.Replace("·", "");
            result = result.Replace("∙", " ");
            result = result.Replace("′", "");
            result = result.Replace("΄", "");
            result = result.Replace("ʼ", "'");
            result = result.Replace("’", "'");
            result = result.Replace("?", "");
            return result;
        }
    }
}
