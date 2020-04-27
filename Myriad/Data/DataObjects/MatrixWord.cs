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

        public int Start { get; private set; }
        public int End { get; private set; }
        public string Text { get; private set; }
        public int Length { get; private set; }
        public int Weight { get; private set; }
        public bool Substitute { get; private set; }

        public int ParameterCount => 5;

        public MatrixWord()
        {
        }
        public MatrixWord(string inflectionString, int id)
        {
            Start = id;
            SetInfo(inflectionString);
            End = Start + Length - 1;
        }

        public object GetParameter(int index) 
        {
            switch (index)
            {
                case Ordinals.first:
                    return Start;
                case Ordinals.second:
                    return End;
                case Ordinals.third:
                    return Substitute;
                case Ordinals.fourth:
                    return Weight;
                case Ordinals.fifth:
                    return Text;
                default:
                    break;
            }
            return null;
        }

        public async Task Read(DbDataReader reader)
        {
            Start = await reader.GetFieldValueAsync<int>(Ordinals.first);
            End = await reader.GetFieldValueAsync<int>(Ordinals.second);
            Substitute = await reader.GetFieldValueAsync<int>(Ordinals.third) > 0;
            Weight = await reader.GetFieldValueAsync<int>(Ordinals.fourth);
            Text = await reader.GetFieldValueAsync<string>(Ordinals.fifth);
        }

        public void ReadSync(DbDataReader reader)
        {
            Start = reader.GetFieldValue<int>(Ordinals.first);
            End = reader.GetFieldValue<int>(Ordinals.second);
            Substitute = reader.GetFieldValue<int>(Ordinals.third) > 0;
            Weight = reader.GetFieldValue<int>(Ordinals.fourth);
            Text = reader.GetFieldValue<string>(Ordinals.fifth);
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            if (End-Start > 0)
            {
                result.Append(End-Start+1);
                if ((!Substitute) && (Weight == keywordWeight)) result.Append("+");
            }
            if (Substitute) result.Append("[");
            if (Weight == commonWordWeight) result.Append("!");
            if (Weight == tagWeight) result.Append("#");
            if (Weight == specialWordWeight) result.Append("*");
            if (Weight == inflectionWeight) result.Append("~");
            if (Weight == originalWordWeight) result.Append("/");
            if (Weight == textLinkWeight) result.Append("/>");
            if (Weight == notTag) result.Append("!#");
            if (Weight == 0) result.Append("-");
            result.Append(Text);
            if (Substitute) result.Append("]");
            return result.ToString();
        }

        private void SetInfo(string inflection)
        {
            if (((inflection[0] > '1') && (inflection[0] <= '9')) && (inflection.Length > 1) && ((inflection[Ordinals.second] == '!') || (inflection[Ordinals.second] == '#') ||
                    (inflection[Ordinals.second] == '[') || (inflection[Ordinals.second] == '~') ||
                    (inflection[Ordinals.second] == '+') || (inflection[Ordinals.second] == '-') || (inflection[Ordinals.second] == '/') || (inflection[Ordinals.second] == '*')))
            {
                Length = inflection[0] - '0';
                inflection = inflection.Substring(1);
            }
            else Length = 1;
            if ((inflection[Ordinals.first] == '[') && (inflection[Ordinals.last] == ']'))
            {
                Substitute = true;
                inflection = inflection[Ordinals.second..Ordinals.nexttolast];
            }
            if (((inflection[0] >= 'A') && (inflection[0] <= 'Z')) || ((inflection[0] >= 'a') && (inflection[0] <= 'z')))
            {
                Weight = keywordWeight;
            }
            else
            {
                if ((inflection.Length > 1) && (inflection.Substring(Ordinals.first, 2) == "/>"))
                {
                    Weight = textLinkWeight;
                    inflection = inflection[Ordinals.third..Ordinals.last];
                }
                if ((inflection.Length > 1) && (inflection.Substring(Ordinals.first, 2) == "!#"))
                {
                    Weight = notTag;
                    inflection = inflection[Ordinals.third..Ordinals.last];
                }
                if (inflection[0] == '+')
                {
                    Weight = keywordWeight;
                    inflection = inflection.Substring(1);
                }
                if (inflection[0] == '!')
                {
                    Weight = commonWordWeight;
                    inflection = inflection.Substring(1);
                }
                if (inflection[0] == '*')
                {
                    Weight = specialWordWeight;
                    inflection = inflection.Substring(1);
                }
                if (inflection[0] == '#')
                {
                    Weight = tagWeight;
                    inflection = inflection.Substring(1);
                }
                if (inflection[0] == '~')
                {
                    Weight = inflectionWeight;
                    inflection = inflection.Substring(1);
                }
                if (inflection[0] == '-')
                {
                    Weight = 0;
                    inflection = inflection.Substring(1);
                }
                if (inflection[0] == '/')
                {
                    Weight = originalWordWeight;
                    inflection = inflection.Substring(1);
                }
            }
            Text = inflection.Replace('\'', '’').Replace('`', '’');
        }

    }
}