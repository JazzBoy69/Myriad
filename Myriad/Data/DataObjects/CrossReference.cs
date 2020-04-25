using System.Data.Common;
using System.Threading.Tasks;
using Feliciana.Data;
using Feliciana.Library;

namespace Myriad.Data
{
    public class CrossReference : DataObject
    {
        public int ArticleID { get; private set; }
        public int ParagraphIndex { get; private set; }
        public int StartID { get; private set; }
        public int EndID { get; private set; }

        public int ParameterCount => 4;

        public CrossReference(int articleID, int paragraphIndex, int startID, int endID)
        {
            ArticleID = articleID;
            ParagraphIndex = paragraphIndex;
            StartID = startID;
            EndID = endID;
        }

        public void Create(DbCommand command)
        {
            throw new System.NotImplementedException();
        }

        public object GetParameter(int index)
        {
            switch (index)
            {
                case Ordinals.first:
                    return ArticleID;
                case Ordinals.second:
                    return this.ParagraphIndex;
                case Ordinals.third:
                    return StartID;
                case Ordinals.fourth:
                    return EndID;
                default:
                    break;
            }
            return null;
        }

        public async Task Read(DbDataReader reader)
        {
            ArticleID = await reader.GetFieldValueAsync<int>(Ordinals.first);
            ParagraphIndex = await reader.GetFieldValueAsync<int>(Ordinals.second);
            StartID = await reader.GetFieldValueAsync<int>(Ordinals.third);
            EndID = await reader.GetFieldValueAsync<int>(Ordinals.fourth);
        }

        public void ReadSync(DbDataReader reader)
        {
            ArticleID = reader.GetFieldValue<int>(Ordinals.first);
            ParagraphIndex = reader.GetFieldValue<int>(Ordinals.second);
            StartID = reader.GetFieldValue<int>(Ordinals.third);
            EndID = reader.GetFieldValue<int>(Ordinals.fourth);
        }
    }
}