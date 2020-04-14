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
                }
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
    }
}
