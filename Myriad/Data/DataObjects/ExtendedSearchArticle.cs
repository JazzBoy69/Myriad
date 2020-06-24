using Feliciana.Data;
using Feliciana.Library;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Myriad.Data
{
    internal class ExtendedSearchArticle : DataObject
    {
        int start;
        int end;
        int articleID;

        public int ParameterCount => 3;
        public int Start => start;

        public int End => end;

        public int ID => articleID;

        public object GetParameter(int index)
        {
            throw new System.NotImplementedException();
        }

        public async Task Read(DbDataReader reader)
        {
            articleID = await reader.GetFieldValueAsync<int>(Ordinals.first);
            start = await reader.GetFieldValueAsync<int>(Ordinals.second);
            end = await reader.GetFieldValueAsync<int>(Ordinals.third);
        }

        public void ReadSync(DbDataReader reader)
        {
            articleID = reader.GetFieldValue<int>(Ordinals.first);
            start = reader.GetFieldValue<int>(Ordinals.second);
            end = reader.GetFieldValue<int>(Ordinals.third);
        }
    }
}