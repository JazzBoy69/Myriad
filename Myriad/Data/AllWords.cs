﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.Data;

namespace Myriad.Data
{
    public class AllWords
    {
        internal static async Task<string> Conform(string query)
        {
            if (query == null) return "";
            query = query.Replace('\'', '’');
            string[] words = query.Split(new char[] { ' ' });
            StringBuilder result = new StringBuilder();
            for (int index = Ordinals.first; index<words.Length; index++)
            {
                if (string.IsNullOrEmpty(words[index])) return query;
                if (index > Ordinals.first) result.Append(' ');
                if (await DataRepository.WordExists(words[index]))
                {
                    result.Append(words[index]);
                    continue;
                }
                string newWord = Symbols.Capitalize(words[index]);
                if (await DataRepository.WordExists(newWord))
                {
                    result.Append(newWord);
                    continue;
                }
                newWord = words[index].ToLower();
                if (await DataRepository.WordExists(newWord))
                {
                    result.Append(newWord);
                    continue;
                }
                newWord = await DataRepository.CorrectSpellingOf(words[index]);
                if (newWord != null)
                {
                    result.Append(newWord);
                    continue;
                }
                result.Append(GetClosestMatch(words[index]));
                result.Append(' ');
            }
            if (result.Length == 0) return "";
            return result.ToString();
        }

        internal static async Task<string> GetClosestMatch(string word)
        {
            if (word == null) return null;
            string posibleResult = word;
            int distance = 2000;
            if (word.Length > 1)
            {
                var words = await DataRepository.TextOfAllSynonyms();
                for (int i=Ordinals.first; i<words.Count; i++)
                {
                    int d = DistanceBetween(word, words[i]);
                    if (d == 1) return words[i];
                    if (d < distance)
                    {
                        distance = d;
                        posibleResult = words[i];
                    }
                }
            }
            if (distance < 3) return posibleResult;
            var allwords = await DataRepository.ReadAllWords();
            for (int i=Ordinals.first; i<allwords.Count; i++)
            {
                int d = DistanceBetween(word, allwords[i]);
                if (d == 1) return allwords[i];
                if (d < distance)
                {
                    distance = d;
                    posibleResult = allwords[i];
                }
            }

            return posibleResult;
        }
        private static int DistanceBetween(string phrase, string word)
        {
            int n = phrase.Length;
            int m = word.Length;
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
                    int cost = (word[j - 1] == phrase[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}
