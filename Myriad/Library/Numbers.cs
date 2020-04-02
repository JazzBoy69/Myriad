using System.Linq;
namespace Myriad.Library
{
    public struct Number
    {
        public const int nothing = 0;
        public const int single = 1;
    }
    public static class Numbers
    {
        internal static bool CompleteCheckIsNumber(string verseString)
        {
            foreach (char c in verseString)
            {
                if (!IsNumber(c)) return false;
            }
            return true;
        }
        public static bool IsNumber(char p)
        {
            return ((p >= '0') && (p <= '9'));
        }
        public static bool IsNumber(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return (IsNumber(text[Ordinals.first]) && IsNumber(text.Last()));
        }
        internal static bool ContainsNumbers(string query)
        {
            foreach (char c in query) if (IsNumber(c)) return true;
            return false;
        }

        public static int Convert(string s)
        {
            if (int.TryParse(s, out int result)) return result;
            return Result.error;
        }
    }

    public static class Result
    {
        public const int nothing = 0;
        public const int error = -1;
        public const int notfound = -1;
    }
    public struct Ordinals
    {
        public const int first = 0;
        public const int second = 1;
        public const int third = 2;
        public const int fourth = 3;
        public const int fifth = 4;
        public const int sixth = 5;
        public const int seventh = 6;
        public const int eighth = 7;
        public const int ninth = 8;
        public const int tenth = 9;
        public const int eleventh = 10;
        public const int twelfth = 11;
        public const int thirteenth = 12;
        public const int fourteenth = 13;
        public const int fifteenth = 14;
        public const int sixteenth = 15;
        public const int seventeenth = 16;
        public const int eighteenth = 17;
        public const int ninteenth = 18;
        public const int twentieth = 19;
        public const int twentyfirst = 20;
        public const int twentysecond = 21;
        public const int twentythird = 22;
        public const int twentyfourth = 23;
        public const int twentyfifth = 24;
        internal static System.Index last = ^1;
        internal static System.Index nexttolast = ^2;
        internal static System.Index secondtolast = ^3;
    }

}