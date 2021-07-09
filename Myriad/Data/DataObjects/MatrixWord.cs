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
        public const int ellipsis = 100;


        int length = -1;

        public int Start { get; private set; }
        public int End { get; private set; }

        public int Length
        {
            get
            {
                if (length != -1) return length;
                return End - Start + 1;
            }
        }
        public string Text { get; private set; }

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
            End = Start + length - 1;
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
            if (Weight == ellipsis) result.Append("..");
            if (Weight == 0) result.Append("-");
            result.Append(Text.Replace(' ', '_'));
            if (Substitute) result.Append("]");
            return result.ToString();
        }

        private void SetInfo(string inflection)
        {
            if (((inflection[Ordinals.first] > '1') && (inflection[Ordinals.first] <= '9')) && (inflection.Length > 1) && 
                ((inflection[Ordinals.second] == '!') || (inflection[Ordinals.second] == '#') ||
                 (inflection[Ordinals.second] == '[') || (inflection[Ordinals.second] == '~') ||
                 (inflection[Ordinals.second] == '+') || (inflection[Ordinals.second] == '-') || 
                 (inflection[Ordinals.second] == '/') || (inflection[Ordinals.second] == '*')))
            {
                length = inflection[Ordinals.first] - '0';
                inflection = inflection.Substring(1);
            }
            else length = 1;
            if ((inflection[Ordinals.first] == '[') && (inflection[Ordinals.last] == ']'))
            {
                Substitute = true;
                inflection = inflection.Substring(Ordinals.second, inflection.Length - 2);
            }
            if (((inflection[Ordinals.first] >= 'A') && (inflection[Ordinals.first] <= 'Z')) || 
                ((inflection[Ordinals.first] >= 'a') && (inflection[Ordinals.first] <= 'z')))
            {
                Weight = keywordWeight;
            }
            else
            {
                if ((inflection.Length > 1) && (inflection.Substring(Ordinals.first, 2) == "/>"))
                {
                    Weight = textLinkWeight;
                    inflection = inflection[Ordinals.third..];
                }
                if ((inflection.Length > 1) && (inflection.Substring(Ordinals.first, 2) == "!#"))
                {
                    Weight = notTag;
                    inflection = inflection[Ordinals.third..];
                }
                if ((inflection.Length > 1) && (inflection.Substring(Ordinals.first, 2) == ".."))
                {
                    Weight = ellipsis;
                    inflection = inflection[Ordinals.third..];
                }
                if (inflection[Ordinals.first] == '+')
                {
                    Weight = keywordWeight;
                    inflection = inflection[Ordinals.second..];
                }
                if (inflection[Ordinals.first] == '!')
                {
                    Weight = commonWordWeight;
                    inflection = inflection[Ordinals.second..];
                }
                if (inflection[Ordinals.first] == '*')
                {
                    Weight = specialWordWeight;
                    inflection = inflection[Ordinals.second..];
                }
                if (inflection[Ordinals.first] == '#')
                {
                    Weight = tagWeight;
                    inflection = inflection[Ordinals.second..];
                }
                if (inflection[Ordinals.first] == '~')
                {
                    Weight = inflectionWeight;
                    inflection = inflection[Ordinals.second..];
                }
                if (inflection[Ordinals.first] == '-')
                {
                    Weight = 0;
                    inflection = inflection[Ordinals.second..];
                }
                if (inflection[Ordinals.first] == '/')
                {
                    Weight = originalWordWeight;
                    inflection = inflection[Ordinals.second..];
                }
            }
            Text = inflection.Replace('\'', '’').Replace('`', '’').Replace('_', ' ');
        }

    }
}