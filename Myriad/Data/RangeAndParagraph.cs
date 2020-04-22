using Feliciana.Data;
using Feliciana.Library;
using Myriad.Library;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Data
{
    public class RangeAndParagraph : DataObject
    {
        int startid;
        int endid;
        int articleid;
        int paragraphindex;
        int offset = 0;
        CitationRange range;

        public int StartID
        {
            get { return startid; }
        }
        public int EndID
        {
            get
            {
                return endid;
            }
        }

        public int ArticleID
        {
            get { return articleid; }
        }
        public int ParagraphIndex
        {
            get { return paragraphindex; }
        }

        public (int articleID, int paragraphIndex) Key
        {
            get { return (articleid, paragraphindex); }
        }

        public (int start, int end) Range { get { return (startid, endid); } }

        internal void SetStartID(int startID)
        {
            offset = this.startid - startID;
            this.startid = startID;
            range = new CitationRange(startid, endid);
        }

        public async Task Read(DbDataReader reader)
        {
            startid = await reader.GetFieldValueAsync<int>(Ordinals.first);
            endid = await reader.GetFieldValueAsync<int>(Ordinals.second);
            articleid = await reader.GetFieldValueAsync<int>(Ordinals.third);
            paragraphindex = await reader.GetFieldValueAsync<int>(Ordinals.fourth);
        }

        public void ReadSync(DbDataReader reader)
        {
            startid = reader.GetFieldValue<int>(Ordinals.first);
            endid = reader.GetFieldValue<int>(Ordinals.second);
            articleid = reader.GetFieldValue<int>(Ordinals.third);
            paragraphindex = reader.GetFieldValue<int>(Ordinals.fourth);
        }

        public void Create(DbCommand command)
        {
            throw new NotImplementedException();
        }

        public object GetParameter(int index)
        {
            throw new NotImplementedException();
        }

        public bool IsOffset
        {
            get { return offset != 0; }
        }

        public int Offset
        {
            get { return offset; }
        }

        public Range OffsetRange
        {
            get { return new Range(startid + offset, endid); }
        }

        public int ParameterCount => throw new NotImplementedException();
    }
}
