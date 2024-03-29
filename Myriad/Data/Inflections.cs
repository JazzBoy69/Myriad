﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;

namespace Myriad.Data
{
    public class Inflections
    {
        internal static async Task<List<string>> RootsOf(string word)
        {
            if (word.Contains(' ')) return await Phrases.RootsOf(word);
            word = RemoveNameDiacritics(word);
            int index = word.IndexOfAny(Symbols.apostrophes);
            word = word.Replace("'", "’").Replace('`', '’').Replace("’s", "").Replace("’", "");

            if (index > Result.notfound) return new List<string>() { word };
            List<string> roots = await EnglishRootsOf(word);
            if (roots.Count > 0) return roots;
            if (EnglishDictionary.IsCommonWord(word))
            {
                if (EnglishDictionary.CommonInflection(word) != word) return new List<string>()
                    { EnglishDictionary.CommonInflection(word) };
            }
            return new List<string>() { word };
        }
        internal static async Task<List<string>> HardRootsOf(string word)
        {
            if (word.Contains(' ')) return new List<string>() { word };
            int index = word.IndexOf('`');
            if (index == Result.notfound)
            {
                index = word.IndexOf('\'');
            }
            word = word.Replace("'", "’").Replace("’s", "").Replace("’", "");
            if (index == Result.notfound)
            {
                if (RemoveDiacritics(word) != word) return new List<string>() { RemoveDiacritics(word) };
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
                return await EnglishRootsOfPhrase (word);
            }
            List<string> results = await DataRepository.RootsOf(word);
            for (int index = Ordinals.first; index < results.Count; index++)
            {
                results[index] = RemoveDiacritics(results[index]);
            }
            if (results.Count == 0) results.Add(RemoveDiacritics(word));
            return results.Distinct().ToList();
        }

        private static async Task<List<string>> EnglishRootsOfPhrase(string phrase)
        {
            List<string> result = await DataRepository.RootsOf(phrase.Replace('_', ' '));
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
            return result.Distinct().ToList();
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

        internal static string RemoveDiacritics(string word)
        {
            string result = word.Replace("·", "");
            result = result.Replace("∙", " ");
            result = result.Replace("΄", "");
            result = result.Replace("΄", "");
            result = result.Replace("ʹ", "");
            result = result.Replace("ʼ", "");
            result = result.Replace("’", "");
            result = result.Replace("′", "");
            result = result.Replace("?", "");
            result = result.Replace("'", "");
            return result;
        }
    }
}
