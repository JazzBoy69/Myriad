using Feliciana.Data;
using Feliciana.Library;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Myriad.Data
{
    //todo clean up dataobject interface
    internal class RelatedTag : DataObject
    {
        internal int ArticleID { get; private set; }
        internal int ParagraphIndex { get; private set; }
        internal int RelatedID { get; private set; }
        public int ParameterCount => 3;

        internal RelatedTag(int articleID, int paragraphIndex, int relatedID)
        {
            ArticleID = articleID;
            ParagraphIndex = paragraphIndex;
            RelatedID = relatedID;
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
                    return RelatedID;
                default:
                    break;
            }
            return null;
        }

        public async Task Read(DbDataReader reader)
        {
            ArticleID = await reader.GetFieldValueAsync<int>(Ordinals.first);
            ParagraphIndex = await reader.GetFieldValueAsync<int>(Ordinals.second);
            RelatedID = await reader.GetFieldValueAsync<int>(Ordinals.third);
        }

        public void ReadSync(DbDataReader reader)
        {
            ArticleID = reader.GetFieldValue<int>(Ordinals.first);
            ParagraphIndex = reader.GetFieldValue<int>(Ordinals.second);
            RelatedID = reader.GetFieldValue<int>(Ordinals.third);
        }
    }
}