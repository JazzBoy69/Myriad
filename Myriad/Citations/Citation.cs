using FelicianaLibrary;

namespace Myriad.Library
{
    public enum CitationTypes
    {
        Chapter, Text, Verse, Invalid
    }
    public class Citation
    {
        public StringRange DisplayLabel = new StringRange();
        public StringRange Label = new StringRange();
        public StringRange LeadingSymbols = new StringRange();
        public StringRange TrailingSymbols = new StringRange();
        public CitationRange CitationRange = CitationRange.InvalidRange;
        public CitationTypes CitationType = CitationTypes.Invalid;

        public Citation()
        {
            DisplayLabel.Invalidate();
        }

        public Citation(int start, int end)
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
                var citation = new Citation(-1, -1, -1);
                citation.CitationType = CitationTypes.Invalid;
                return citation;
            }
        }

        internal Citation Copy()
        {
            Citation newCitation = new Citation();
            newCitation.Label = new StringRange(Label.Start, Label.End);
            newCitation.CitationRange = new CitationRange(CitationRange.StartID,
                CitationRange.EndID);
            newCitation.CitationType = CitationType;
            newCitation.LeadingSymbols = new StringRange(LeadingSymbols.Start, LeadingSymbols.End);
            newCitation.TrailingSymbols = new StringRange(TrailingSymbols.Start, TrailingSymbols.End);
            return newCitation;
        }

        internal void Set(VerseReference firstVerse, VerseReference secondVerse)
        {
            if (secondVerse.WordIndex != Result.notfound)
            {
                CitationRange.Set(firstVerse.Book, firstVerse.Chapter, firstVerse.Verse,
                    firstVerse.WordIndex, secondVerse.Chapter, secondVerse.Verse,
                    secondVerse.WordIndex);

                CitationType = CitationTypes.Text;
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
                CitationType = CitationTypes.Text;
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
