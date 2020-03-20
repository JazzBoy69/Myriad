using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Parser
{
    public class VerseReference
    {
        public int Book = Result.notfound;
        public int Chapter = Result.notfound;
        public int Verse = Result.notfound;
        public int WordIndex = Result.notfound;

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
                Second.Set(index, value);
            }
            else
            {
                First.Set(index, value);
            }
        }

    }
}
