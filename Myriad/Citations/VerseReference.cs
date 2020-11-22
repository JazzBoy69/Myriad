using Feliciana.Library;

namespace Myriad.Library
{
    public class VerseReference
    {
        public int Book = Result.notfound;
        public int Chapter = Result.notfound;
        public int Verse = Result.notfound;
        public int WordIndex = Result.notfound;

        public VerseReference()
        {

        }
        public VerseReference(int book, int chapter, int verse, int word)
        {
            Book = book;
            Chapter = chapter;
            Verse = verse;
            WordIndex = word;
        }
        public void Set(int index, int value)
        {
            switch (index)
            {
                case 0:
                    Book = value;
                    break;
                case 1:
                    Chapter = value;
                    break;
                case 2:
                    Verse = value;
                    break;
                case 3:
                    WordIndex = value;
                    break;
                default:
                    break;
            }
        }
        public void Reset()
        {
            Book = Result.notfound;
            Chapter = Result.notfound;
            Verse = Result.notfound;
            WordIndex = Result.notfound;
        }
    }

    public enum CitedVerseIndex
    {
        firstBook, firstChapter, firstVerse, firstWord,
        secondChapter, secondVerse, secondWord
    }
    public class CitedVerse
    {
        public VerseReference First = new VerseReference();
        public VerseReference Second = new VerseReference();

        public void Set(int index, int value)
        {
            if (index > Ordinals.fourth)
            {
                index -= 4;
                if ((index == Ordinals.third) && (Second.Chapter == Result.notfound))
                {
                    Second.Chapter = First.Chapter;
                }
                if ((index == Ordinals.second) && (Bible.IsShortBook(First.Book)))
                {
                    Second.Chapter = 1;
                    index++;
                }
                Second.Set(index, value);
            }
            else
            {
                if ((index == Ordinals.second) && (Bible.IsShortBook(First.Book)))
                {
                    First.Chapter = 1;
                    index++;
                }
                First.Set(index, value);
            }
        }

    }
}
