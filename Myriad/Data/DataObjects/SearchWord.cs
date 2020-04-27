using Feliciana.Data;
using Feliciana.Library;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Data
{
    public class SearchWord : DataObject
    {
        int sentenceID;
        int wordIndex;
        int weight;
        int start;
        int end;
        int substitute;
        string text;

        public int SentenceID => sentenceID;
        public int WordIndex => wordIndex;
        public int Weight => weight;
        public int Start => start;
        public int End => end;
        public int Substitute => substitute;
        public string Text => text;
        public int ParameterCount => throw new NotImplementedException();

        public object GetParameter(int index)
        {
            throw new NotImplementedException();
        }

        public async Task Read(DbDataReader reader)
        {
            text = await reader.GetFieldValueAsync<string>(Ordinals.first);
            sentenceID = await reader.GetFieldValueAsync<int>(Ordinals.second);
            wordIndex = await reader.GetFieldValueAsync<int>(Ordinals.third);
            weight = await reader.GetFieldValueAsync<int>(Ordinals.fourth);
            start = await reader.GetFieldValueAsync<int>(Ordinals.fifth);
            end = await reader.GetFieldValueAsync<int>(Ordinals.sixth);
            substitute = await reader.GetFieldValueAsync<int>(Ordinals.seventh);
        }

        public void ReadSync(DbDataReader reader)
        {
            text = reader.GetFieldValue<string>(Ordinals.first);
            sentenceID = reader.GetFieldValue<int>(Ordinals.second);
            wordIndex = reader.GetFieldValue<int>(Ordinals.third);
            weight = reader.GetFieldValue<int>(Ordinals.fourth);
            start = reader.GetFieldValue<int>(Ordinals.fifth);
            end = reader.GetFieldValue<int>(Ordinals.sixth);
            substitute = reader.GetFieldValue<int>(Ordinals.seventh);
        }
    }
}
