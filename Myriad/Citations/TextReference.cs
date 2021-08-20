using Feliciana.Library;
namespace Myriad.Library
{
    public class TextReference
    {
        public const int invalidBook = 255;
        public const int invalidChapter = 255;
        public const int invalidVerse = 255;
        internal static int IndexOfBook(string p)
        {
            if (Bible.NamesTitleCaseIndex.TryGetValue(p, out int result))
                return result;
            if (Bible.NamesIndex.TryGetValue(p.ToUpper(), out result))
                return result;
            return invalidBook;
        }

        public static bool IsShortBook(int book)
        {
            return ((book == 30) || (book == 56) || ((book > 61) && (book < 65)));
        }
    }
}
