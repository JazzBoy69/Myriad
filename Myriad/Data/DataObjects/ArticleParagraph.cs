using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Data
{
    public class ArticleParagraph : DataObject
    {
        int id;
        int index;
        string text;
        public int ParameterCount => 3;

        public ArticleParagraph(int id, int index, string text)
        {
            this.id = id;
            this.index = index;
            this.text = text;
        }

        public void AddParameterTo<DataType>(DataWriter<DataType> writer, int index) where DataType : DataObject
        {
            switch (index)
            {
                case Ordinals.first:
                    writer.AddParameter<int>(index, id);
                    break;
                case Ordinals.second:
                    writer.AddParameter<int>(index, this.index);
                    break;
                case Ordinals.third:
                    writer.AddParameter<string>(index, text);
                    break;
                default:
                    break;
            }
        }

        public void Create(DbCommand command)
        {
            throw new NotImplementedException();
        }

        public void Read(DbDataReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
