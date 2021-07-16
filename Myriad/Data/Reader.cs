using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feliciana.Data;
using Feliciana.Library;
using Myriad.Library;

namespace Myriad.Data
{
    public class Reader
    {
        public static async Task<string> ReadTitle(int id)
        {
            var titleReader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadArticleTitle), id);
            string title = await titleReader.GetDatum<string>();
            titleReader.Close();
            return title;
        }
        public static List<Keyword> ReadKeywords(Citation citation)
        {
            var reader = new StoredProcedureProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadKeywords),
                citation.CitationRange.StartID.ID, citation.CitationRange.EndID.ID);
            var result = reader.GetClassData<Keyword>();
            reader.Close();
            return result;
        }

        public static List<RubyInfo> ReadSustituteText(int id)
        {
            var reader = new StoredProcedureProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadSubstituteWords),
                id);
            List<RubyInfo> substituteWords = reader.GetClassData<RubyInfo>();
            reader.Close();
            return substituteWords;
        }

        public static async Task<string> ReadSustituteWord(int id)
        {
            var reader = new StoredProcedureProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadSubstituteWords),
                id);
            List<(string, int)> substituteWords = await reader.GetData<string, int>();
            reader.Close();
            return (substituteWords.Count > Number.nothing) ?
                substituteWords[Ordinals.first].Item1 :
                "";
        }

    }
}
