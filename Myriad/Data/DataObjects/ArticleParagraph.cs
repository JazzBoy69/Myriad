using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.Data;

namespace Myriad.Data
{
    public class ArticleParagraph : DataObject
    {
        private int id;
        private int index;
        private string text;
        public int ParameterCount => 3;

        public ArticleParagraph(int id, int index, string text)
        {
            this.id = id;
            this.index = index;
            this.text = text;
        }

        public void Create(DbCommand command)
        {
            throw new NotImplementedException();
        }

        public async Task Read(DbDataReader reader)
        {
            id = await reader.GetFieldValueAsync<int>(Ordinals.first);
            index = await reader.GetFieldValueAsync<int>(Ordinals.second);
            text = await reader.GetFieldValueAsync<string>(Ordinals.third);
        }

        public object GetParameter(int index)
        {
            switch (index)
            {
                case Ordinals.first:
                    return id;
                case Ordinals.second:
                    return this.index;
                case Ordinals.third:
                    return text;
                default:
                    break;
            }
            return null;
        }
    }
}
