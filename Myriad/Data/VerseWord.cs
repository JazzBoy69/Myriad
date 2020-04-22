using Feliciana.Data;
using Feliciana.Library;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Data
{
    public class VerseWord : DataObject
    {
        int start;
        int end;
        string text;
        int weight;
        bool substitute;
        public int Start => start;
        public int End => end;
        public (int start, int end) Range => (start, end);
        public string Text => text;
        public int Weight => weight;
        public bool Substitute => substitute;
        public int ParameterCount => throw new NotImplementedException();

        public void Create(DbCommand command)
        {
            throw new NotImplementedException();
        }

        public object GetParameter(int index)
        {
            throw new NotImplementedException();
        }

        public async Task Read(DbDataReader reader)
        {
            start = await reader.GetFieldValueAsync<int>(Ordinals.first);
            end = await reader.GetFieldValueAsync<int>(Ordinals.second);
            text = await reader.GetFieldValueAsync<string>(Ordinals.third);
            weight = await reader.GetFieldValueAsync<int>(Ordinals.fourth);
            substitute = await reader.GetFieldValueAsync<int>(Ordinals.fifth) > 0;
        }

        public void ReadSync(DbDataReader reader)
        {
            start = reader.GetFieldValue<int>(Ordinals.first);
            end = reader.GetFieldValue<int>(Ordinals.second);
            text = reader.GetFieldValue<string>(Ordinals.third);
            weight = reader.GetFieldValue<int>(Ordinals.fourth);
            substitute = reader.GetFieldValue<int>(Ordinals.fifth) > 0;
        }
    }
}
