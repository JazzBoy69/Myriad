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
        public int ParameterCount => 3;
        public int ID { get; private set; }
        public string Text { get; private set; }
        public int ParagraphIndex { get; private set; }
        public ArticleParagraph(int id, int index, string text)
        {
            this.ID = id;
            this.ParagraphIndex = index;
            this.Text = text;
        }

        public void Create(DbCommand command)
        {
            throw new NotImplementedException();
        }

        public async Task Read(DbDataReader reader)
        {
            ID = await reader.GetFieldValueAsync<int>(Ordinals.first);
            ParagraphIndex = await reader.GetFieldValueAsync<int>(Ordinals.second);
            Text = await reader.GetFieldValueAsync<string>(Ordinals.third);
        }

        public object GetParameter(int index)
        {
            switch (index)
            {
                case Ordinals.first:
                    return ID;
                case Ordinals.second:
                    return this.ParagraphIndex;
                case Ordinals.third:
                    return Text;
                default:
                    break;
            }
            return null;
        }

        public void ReadSync(DbDataReader reader)
        {
            ID =  reader.GetFieldValue<int>(Ordinals.first);
            ParagraphIndex =  reader.GetFieldValue<int>(Ordinals.second);
            Text =  reader.GetFieldValue<string>(Ordinals.third);
        }
    }
}
