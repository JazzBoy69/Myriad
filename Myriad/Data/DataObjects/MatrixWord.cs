using System.Text;
using System.Data.Common;
using System.Threading.Tasks;
using Feliciana.Data;
using Feliciana.Library;

namespace Myriad.Data
{
    internal class MatrixWord : DataObject
    {
        public const ushort keywordWeight = 500;
        public const ushort specialWordWeight = 3000;
        public const ushort tagWeight = 5000;
        public const ushort commonWordWeight = 10;
        public const ushort textLinkWeight = 96;
        public const ushort inflectionWeight = 300;
        public const ushort originalWordWeight = 200;
        public const int notTag = -100;

        int start;
        int end;
        bool substitute;
        int weight;
        string text;

        public int ParameterCount => 5;

        public object GetParameter(int index)
        {
            switch (index)
            {
                case Ordinals.first:
                    return start;
                case Ordinals.second:
                    return end;
                case Ordinals.third:
                    return substitute;
                case Ordinals.fourth:
                    return weight;
                case Ordinals.fifth:
                    return text;
                default:
                    break;
            }
            return null;
        }

        public async Task Read(DbDataReader reader)
        {
            start = await reader.GetFieldValueAsync<int>(Ordinals.first);
            end = await reader.GetFieldValueAsync<int>(Ordinals.second);
            substitute = await reader.GetFieldValueAsync<int>(Ordinals.third) > 0;
            weight = await reader.GetFieldValueAsync<int>(Ordinals.fourth);
            text = await reader.GetFieldValueAsync<string>(Ordinals.fifth);
        }

        public void ReadSync(DbDataReader reader)
        {
            start = reader.GetFieldValue<int>(Ordinals.first);
            end = reader.GetFieldValue<int>(Ordinals.second);
            substitute = reader.GetFieldValue<int>(Ordinals.third) > 0;
            weight = reader.GetFieldValue<int>(Ordinals.fourth);
            text = reader.GetFieldValue<string>(Ordinals.fifth);
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            if (end-start > 0)
            {
                result.Append(end-start+1);
                if ((!substitute) && (weight == keywordWeight)) result.Append("+");
            }
            if (substitute) result.Append("[");
            if (weight == commonWordWeight) result.Append("!");
            if (weight == tagWeight) result.Append("#");
            if (weight == specialWordWeight) result.Append("*");
            if (weight == inflectionWeight) result.Append("~");
            if (weight == originalWordWeight) result.Append("/");
            if (weight == textLinkWeight) result.Append("/>");
            if (weight == notTag) result.Append("!#");
            if (weight == 0) result.Append("-");
            result.Append(text);
            if (substitute) result.Append("]");
            return result.ToString();
        }
    }
}