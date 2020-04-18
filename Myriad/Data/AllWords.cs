using System;
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
                if (await WordExists(words[index]))
                {
                    result.Append(words[index]);
                    continue;
                }
                string newWord = Symbols.Capitalize(words[index]);
                if (await WordExists(newWord))
                {
                    result.Append(newWord);
                    continue;
                }
                newWord = words[index].ToLower();
                if (await WordExists(newWord))
                {
                    result.Append(newWord);
                    continue;
                }
                result.Append(GetClosestMatch(newWord));
                result.Append(' ');
            }
            if (result.Length == 0) return "";
            return result.ToString();
        }

        internal static async Task<bool> WordExists(string word)
        {
            var reader = new DataReaderProvider<string>(
                SqlServerInfo.GetCommand(DataOperation.ReadFromAllWords), word);
            string result = await reader.GetDatum<string>();
            reader.Close();
            return result != null;
        }

        internal static string GetClosestMatch(string word)
        {
            string posibleResult = word;
            var roots = Inflections.RootsOf(word);
            if (roots.Contains(word)) return word;
            int distance = 2000;
            if (word.Length > 1)
            {
                var reader = new DataReaderProvider(
                    SqlServerInfo.CreateCommandFromQuery("select RTrim(text) from synonyms"));
                var words = reader.GetData<string>();
                reader.Close();
                foreach (string s in words)
                {
                    int d = DistanceBetween(word, s);
                    if (d == 1) return s;
                    if (d < distance)
                    {
                        distance = d;
                        posibleResult = s;
                    }
                }
            }
            if (distance < 3) return posibleResult;
            var allreader = new DataReaderProvider(
                SqlServerInfo.CreateCommandFromQuery("select RTrim(text) from allwords"));
            var allwords = allreader.GetData<string>();
            allreader.Close();
            foreach (string w in allwords)
            {
                int d = DistanceBetween(word, w);
                if (d == 1) return w;
                if (d < distance)
                {
                    distance = d;
                    posibleResult = w;
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
