using System;
using Feliciana.Library;

namespace Myriad.Library
{
    public enum CitationTypes
    {
        Chapter, Text, Verse, Invalid
    }
    public enum LabelTypes
    {
        Long, Short, Normal
    }
    public class Citation
    {
        public StringRange DisplayLabel = new StringRange();
        public StringRange Label = new StringRange();
        public StringRange LeadingSymbols = new StringRange();
        public StringRange TrailingSymbols = new StringRange();
        public CitationRange CitationRange = CitationRange.InvalidRange();
        public CitationTypes CitationType = CitationTypes.Invalid;
        public LabelTypes LabelType = LabelTypes.Short;
        public bool Navigating = false;

        public Citation()
        {
            DisplayLabel.Invalidate();
        }

        public Citation(int start, int end)
        {
            CitationRange = new CitationRange(start, end);
        }

        public Citation(KeyID start, KeyID end)
        {
            CitationRange = new CitationRange(start, end);
        }

        public Citation(int book, int chapter, int verse)
        {
            CitationRange = new CitationRange(book, chapter, verse);
            CitationType = CitationTypes.Text;
        }
        public static Citation InvalidCitation
        {
            get
            {
                var citation = new Citation(-1, -1, -1)
                {
                    CitationType = CitationTypes.Invalid
                };
                return citation;
            }
        }

        public override int GetHashCode()
        {
            return CitationRange.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            Citation other = (Citation)obj;
            return CitationRange.StartID.ID == other.CitationRange.StartID.ID &&
                CitationRange.EndID.ID == other.CitationRange.EndID.ID;
        }
        public bool Equals(Citation other)
        {
            if (other == null) return false;
            return CitationRange.StartID.ID == other.CitationRange.StartID.ID &&
                CitationRange.EndID.ID == other.CitationRange.EndID.ID;
        }

        internal Citation Copy()
        {
            Citation newCitation = new Citation
            {
                Label = new StringRange(Label.Start, Label.End),
                CitationRange = new CitationRange(CitationRange.StartID,
                CitationRange.EndID),
                CitationType = CitationType,
                LeadingSymbols = new StringRange(LeadingSymbols.Start, LeadingSymbols.End),
                TrailingSymbols = new StringRange(TrailingSymbols.Start, TrailingSymbols.End)
            };
            return newCitation;
        }

        internal void Set(VerseReference verse)
        {
            if (verse.WordIndex != Result.notfound)
            {
                CitationRange.Set(verse.Book, verse.Chapter, verse.Verse,
                    verse.WordIndex);
                CitationType = CitationTypes.Verse;
                return;
            }
            if (verse.Verse != Result.notfound)
            {
                CitationRange.Set(verse.Book, verse.Chapter, verse.Verse);
                CitationType = CitationTypes.Text;
                return;
            }
            if (verse.Chapter != Result.notfound)
            {
                CitationRange.Set(verse.Book, verse.Chapter);
                CitationType = CitationTypes.Chapter;
                return;
            }
        }
        internal void Set(VerseReference firstVerse, VerseReference secondVerse)
        {
            if (secondVerse.WordIndex != Result.notfound)
            {
                CitationRange.Set(firstVerse.Book, firstVerse.Chapter, firstVerse.Verse,
                    firstVerse.WordIndex, secondVerse.Chapter, secondVerse.Verse,
                    secondVerse.WordIndex);

                CitationType = CitationTypes.Verse;
            }
            if (secondVerse.Verse != Result.notfound)
            {
                if (firstVerse.WordIndex != Result.notfound)
                {
                    CitationRange.Set(firstVerse.Book, firstVerse.Chapter, firstVerse.Verse,
                     firstVerse.WordIndex, firstVerse.Chapter, secondVerse.Verse,
                     KeyID.MaxWordIndex);
                }
                else
                {
                    if (secondVerse.Chapter != Result.notfound)
                        CitationRange.Set(firstVerse.Book, firstVerse.Chapter, firstVerse.Verse,
                         secondVerse.Chapter, secondVerse.Verse);
                    else
                        CitationRange.Set(firstVerse.Book, firstVerse.Chapter, firstVerse.Verse,
                     firstVerse.Chapter, secondVerse.Verse);
                }
                CitationType = CitationTypes.Text;
                return;
            }
            if (secondVerse.Chapter != Result.notfound)
            {
                CitationRange.Set(firstVerse.Book, firstVerse.Chapter, Ordinals.first,
                    secondVerse.Chapter, Bible.Chapters[firstVerse.Book][secondVerse.Chapter]);
                CitationType = CitationTypes.Text;
                return;
            }
            if (firstVerse.WordIndex != Result.notfound)
            {
                CitationRange.Set(firstVerse.Book, firstVerse.Chapter, firstVerse.Verse,
                    firstVerse.WordIndex);
                CitationType = CitationTypes.Verse;
                return;
            }
            if (firstVerse.Verse != Result.notfound)
            {
                CitationRange.Set(firstVerse.Book, firstVerse.Chapter, firstVerse.Verse);
                CitationType = CitationTypes.Text;
                return;
            }
            if (firstVerse.Chapter != Result.notfound)
            {
                CitationRange.Set(firstVerse.Book, firstVerse.Chapter);
                CitationType = CitationTypes.Chapter;
                return;
            }
        }
    }
}
