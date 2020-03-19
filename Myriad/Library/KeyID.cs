using System;

namespace Myriad.Library
{
    public class KeyID
    {
        public const int MaxWordIndex = 255;      
        public const int InvalidBook = Result.error;

        const int bookMulitiplier = 256 * 256 * 256;
        const int chapterMultiplier = 256 * 256;
        const int verseMultiplier = 256;
        const int bookmask = 127 << 24;
        const int chaptermask = 255 << 16;
        const int versemask = 255 << 8;
        const int wordmask = 255;
        int id;
        public KeyID(int? id)
        {
            this.id = id ?? Result.error;
        }

        public KeyID(string id)
        {
            this.id = Numbers.Convert(id);
        }

            public KeyID(int book, int chapter, int verse)
        {
            id = (book * bookMulitiplier) + (chapter * chapterMultiplier) + 
                (verse * verseMultiplier) + Ordinals.first;
        }

        public KeyID(int book, int chapter, int verse, int index)
        {
            id = (book * bookMulitiplier) + (chapter * chapterMultiplier) + 
                (verse * verseMultiplier) + index;
        }

        public void Set(int newID)
        {
            id = newID;
        }

        public int ID
        {
            get
            {
                return id;
            }
        }

        public int Book
        {
            get
            {
                return (id & bookmask) >> 24;
            }
        }

        public int Chapter
        {
            get
            {
                return (id & chaptermask) >> 16;
            }
        }

        public int Verse
        {
            get
            {
                return (id & versemask) >> 8;
            }
        }

        public int WordIndex
        {
            get
            {
                return id & wordmask;
            }
        }

        public Tuple<int, int, int> Key
        {
            get
            {
                return Tuple.Create(Book, Chapter, Verse);
            }
        }

        internal void Set(int book, int chapter, int verse)
        {
            id = book * bookMulitiplier + chapter * chapterMultiplier + verse * verseMultiplier;
        }

        /// <summary>
        /// Compares two KeyIDs.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        internal bool SameVerse(KeyID reference)
        {
            return ((this.Book == reference.Book) && (this.Chapter == reference.Chapter) && (this.Verse == reference.Verse));
        }

        internal bool SameChapter(KeyID reference)
        {
            return ((this.Book == reference.Book) && (this.Chapter == reference.Chapter));
        }

        public bool Valid
        {
            get
            {
                return Book >= 0 && Book <= 65 && Chapter >= 1 && Chapter <= Bible.Chapters[Book].Length &&
                    Verse >= 0 && Verse <= Bible.Chapters[Book][Chapter];
                
            }
        }
    }
}