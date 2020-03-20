using System;
using System.Collections.Generic;
using System.Text;

namespace Myriad.Library
{
    public class CitationRange
    {
        public const int invalidID = Numbers.nothing;
        internal static CitationRange InvalidRange = new CitationRange(invalidID, invalidID);

        KeyID start;
        KeyID end;

        public CitationRange(int? startID, int? endID)
        {
            this.start = new KeyID(startID);
            this.end = new KeyID(endID);
        }

        public CitationRange((int startID, int endID) input)
        {
            this.start = new KeyID(input.startID);
            this.end = new KeyID(input.endID);
        }

        internal CitationRange(Tuple<int, int> tuple)
        {
            this.start = new KeyID(tuple.Item1);
            this.end = new KeyID(tuple.Item2);
        }

        public CitationRange(int id)
        {
            start = new KeyID(id);
            end = new KeyID(id);
        }

        private static bool GoodStrings(string start, string end)
        {
            return !((String.IsNullOrEmpty(start)) && (String.IsNullOrEmpty(end)));
        }
        public CitationRange(string start, string end)
        {
            if (!GoodStrings(start, end))
            {
                this.start = new KeyID(invalidID);
                this.end = new KeyID(invalidID);
            }
            else
            {
                this.start = new KeyID(start);
                this.end = new KeyID(end);
            }
        }

        public CitationRange(int book, int chapter, int verse)
        {
            Set(book, chapter, verse);
        }

        internal void Set(int book, int chapter)
        {
            if (book > 65)
            {
                this.Invalidate();
                return;
            }
            start = new KeyID(book, chapter, 1);
            end = new KeyID(book, chapter, Bible.Chapters[book][chapter]);
        }
        public void Set(int book, int chapter, int verse)
        {
            start = new KeyID(book, chapter, verse);
            end = new KeyID(book, chapter, verse, KeyID.MaxWordIndex);
        }

        public void Set(int book, int chapter, int verse, int wordIndex)
        {
            start = new KeyID(book, chapter, verse, wordIndex);
            end = new KeyID(book, chapter, verse, KeyID.MaxWordIndex);
        }
        internal void Set(int book, int firstChapter, int firstVerse, int lastChapter, int lastVerse)
        {
            start = new KeyID(book, firstChapter, firstVerse, Ordinals.first);
            end = new KeyID(book, lastChapter, lastVerse, KeyID.MaxWordIndex);
        }

        internal void Set(int book, int firstChapter, int firstVerse, int firstWord,
            int lastChapter, int lastVerse, int lastWord)
        {
            if (firstWord == Result.notfound) firstWord = Ordinals.first;
            start = new KeyID(book, firstChapter, firstVerse, firstWord);
            end = new KeyID(book, lastChapter, lastVerse, lastWord);
        }


        public int StartID
        {
            get
            {
                return start.ID;
            }
        }
        public int EndID
        {
            get
            {
                return end.ID;
            }
        }

        public int Book
        {
            get
            {
                return start.Book;
            }
        }

        internal bool Contains(CitationRange targetRange)
        {
            return (targetRange.start.ID >= start.ID) && (targetRange.end.ID <= end.ID);
        }

        internal void ExtendTo(int end)
        {
            this.end.Set(end);
        }


        internal bool Equals(CitationRange otherRange)
        {
            return start.ID == otherRange.start.ID && end.ID == otherRange.end.ID;
        }

        public int Length 
        { 
            get 
            { 
                return end.ID - start.ID + 1; 
            } 
        }

        public (int startID, int endID) Key 
        { 
            get 
            { 
                return (start.ID, end.ID); 
            } 
        }

        public int FirstChapter
        {
            get
            {
                return start.Chapter;
            }
        }

        public int FirstVerse
        {
            get
            {
                return start.Verse;
            }
        }

        public int LastChapter
        {
            get
            {
                return end.Chapter;
            }
        }

        public int LastVerse
        {
            get
            {
                return end.Verse;
            }
        }
        internal void Invalidate()
        {
            start = new KeyID(invalidID);
            end = new KeyID(invalidID);

        }

        public bool Valid
        {
            get
            {
                return start.Valid;
            }
        }

        internal static bool InRange(CitationRange range, CitationRange targetRange)
        {
            if ((range == null) || (targetRange == null)) return false;
            return (targetRange.start.ID <= range.end.ID) && (targetRange.end.ID >= range.start.ID);
        }
    }
}