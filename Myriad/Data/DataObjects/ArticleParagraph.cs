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
        private readonly int id;
        private readonly int index;
        private readonly string text;
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

        public void Read(DbDataReader reader)
        {
            throw new NotImplementedException();
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
