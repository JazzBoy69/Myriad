namespace Myriad.Library
{
    //todo move to Feliciana Library
    public struct Symbol
    {
        public const string lineFeed = "\r\n";
        public const string newLine = "\n";
        public const string returnChar = "\r";
        public const char tab = '\t';
        public const char space = ' ';
        public const string spaceString = " ";
        public const string ellipsis = "&hellip;";
    }
        public static class Symbols
    {
        internal static bool IsLetter(char c)
        {
            return (((c >= 'A') && (c <= 'Z')) || ((c >= 'a') && (c < 'z')));
        }
        internal static string Capitalize(string word)
        {
            if ((word[Ordinals.first] > 96) && (word[Ordinals.first] < 123))
                return (char)(word[Ordinals.first] - 32) + word.Substring(Ordinals.second);
            return word;
        }
        internal static string[] linefeedArray = new string[] { Symbol.lineFeed, Symbol.newLine, Symbol.returnChar };
        internal static char[] tabArray = new char[] { Symbol.tab };
        internal static char[] spaceArray = new char[] { Symbol.space };
        internal static char[] sentencePunctuation = new char[] { '.', '?', '!' };
        public static bool IsPartOfWord(char p)
        {
            if (((p >= 'A') && (p <= 'Z')) || ((p >= 'a') && (p <= 'z')) || (p == '\'') || (p == '`') || (p == '-') || (p == '_') || (p == '(') || (p == ')')) return true;
            return false;
        }
        internal static bool ContainsEndofSentencePunctuation(string s)
        {
            return s.IndexOfAny(sentencePunctuation) != -1;
        }
    }
}
