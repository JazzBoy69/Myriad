using System;
using System.Collections.Generic;
using Myriad.Library;


namespace Myriad.Parser
{
    public enum CitationTypes
    {
        Chapter, Text, Verse, Invalid
    }
    public class Citation
    {
        public StringRange Label = new StringRange();
        public StringRange LeadingSymbols = new StringRange();
        public StringRange TrailingSymbols = new StringRange();
        public CitationRange CitationRange = CitationRange.InvalidRange;
        public CitationTypes CitationType = CitationTypes.Invalid;

        public Citation()
        {
            
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
    }
    public class CitationHandler
    {
        const int findChapter = 0;
        const int secondPart = 1;
        const int findVerse = 2;
        const int afterComma = 3;
        const int findWordIndex = 4;
        const int findLastWordIndex = 5;

        StringRange mainRange;
        Citation citation;
        bool HasLabel { get; set; }

        public List<Citation> ParseCitations(StringRange mainRange, IMarkedUpParagraph currentParagraph)
        {
            this.mainRange = mainRange;
            var result = new List<Citation>();
            bool shouldContinue = true;
            while (shouldContinue)
            {
                var citation = ParseCitation(mainRange, currentParagraph);   
                result.AddRange(citation);
                shouldContinue = false; //TODO: make loop
            }
            return result;
        }

        private List<Citation> ParseCitation(StringRange mainRange, IMarkedUpParagraph paragraph)
        {
            citation = new Citation();
            var results = new List<Citation>();
            citation.Label = GetBookRange(mainRange, paragraph);
            int book = GetBookIndex(citation.Label, paragraph);
            if (book == TextReference.invalidBook)
            {
                results.Add(Citation.InvalidCitation);
                return results;
            }
            int mode = TextReference.IsShortBook(book) ? findVerse : findChapter;
            int count = 0;
            int chapter = TextReference.invalidChapter;
            bool foundZero = false;
            int firstChapter = TextReference.invalidChapter;
            int firstVerse = TextReference.invalidVerse;
            int lastVerse = TextReference.invalidVerse;
            bool first = true;

            citation.Label.BumpEnd();
            while (citation.Label.End < mainRange.End)
            {
                switch (paragraph.CharAt(citation.Label.End))
                {
                    case ' ':
                        if (citation.Label.End - mainRange.Start == 0)
                        {
                            AppendNextStartCharacter();
                        }
                        citation.Label.BumpEnd();
                        continue;
                    case ':':
                        chapter = count;
                        count = 0;
                        foundZero = false;
                        citation.Label.BumpEnd();
                        mode = findVerse;
                        continue;
                    case ',':
                        if (mode == findVerse)
                        {
                            firstVerse = count;
                            count = 0;
                            foundZero = false;
                            mode = afterComma;
                            //citation.Label.MoveStartTo(citation.Label.End);
                            citation.Label.BumpEnd();
                            continue;
                        }
                        citation.Label.BumpEnd();
                        continue;
                    case '0':
                        if (count == 0)
                        {
                            foundZero = true;
                            citation.Label.BumpEnd();
                            continue;
                        }
                        count *= 10;
                        citation.Label.BumpEnd();
                        continue;
                    case '1':
                        count *= 10;
                        count++;
                        citation.Label.BumpEnd();
                        continue;
                    case '2':
                        count *= 10;
                        count += 2;
                        citation.Label.BumpEnd();
                        continue;
                    case '3':
                        count *= 10;
                        count += 3;
                        citation.Label.BumpEnd();
                        continue;
                    case '4':
                        count *= 10;
                        count += 4;
                        citation.Label.BumpEnd();
                        continue;
                    case '5':
                        count *= 10;
                        count += 5;
                        citation.Label.BumpEnd();
                        continue;
                    case '6':
                        count *= 10;
                        count += 6;
                        citation.Label.BumpEnd();
                        continue;
                    case '7':
                        count *= 10;
                        count += 7;
                        citation.Label.BumpEnd();
                        continue;
                    case '8':
                        count *= 10;
                        count += 8;
                        citation.Label.BumpEnd();
                        continue;
                    case '9':
                        count *= 10;
                        count += 9;
                        citation.Label.BumpEnd();
                        continue;
                    default:
                        if ((mode == findWordIndex) || (mode == findLastWordIndex))
                        {
                           /* wordLabel += paragraph.CharAt(citationRange.End);
                            citationRange.BumpEnd(); */
                            continue;
                        }
                        break;
                }
            }
            if (mode == findVerse)
            {
                if ((count != 0) || (foundZero))
                    SetTextCitation(book, chapter, count);
            }
            if (mode == findChapter)
            {
                if ((chapter == TextReference.invalidChapter) && (count > 0))
                {
                    if (TextReference.IsShortBook(book))
                        SetTextCitation(book, 1, count);
                    else
                        SetChapterCitation(book, count);
                }
            }
            if (mode == afterComma)
            {
                if (count == (firstVerse + 1))
                {
                    SetVerseRangeCitation(book, chapter, firstVerse, count);
                }
                else
                {
                    SetTextCitation(book, chapter, firstVerse);
                    results.Add(citation);
                    citation = new Citation();
                    citation.Label.Copy(mainRange);
                    AppendNextStartCharacter();
                    if ((count != 0) || (foundZero))
                        SetTextCitation(book, chapter, count);
                }
            }
            results.Add(citation);
            return results;
        }

        private void SetVerseRangeCitation(int book, int chapter, int firstVerse, int count)
        {
            citation.CitationRange.Set(book, chapter, firstVerse, chapter, count);
            citation.CitationType = CitationTypes.Text;
        }

        private void SetLongCitation(int book, int firstChapter, int firstVerse, int lastChapter, int count)
        {
            citation.CitationRange.Set(book, firstChapter, firstVerse, lastChapter, count);
            citation.CitationType = CitationTypes.Text;
        }

        private void SetChapterCitation(int book, int chapter)
        {
            citation.CitationRange.Set(book, chapter);
            citation.CitationType = CitationTypes.Chapter;
        }

        private void SetTextCitation(int book, int chapter, int count)
        {
            citation.CitationRange.Set(book, chapter, count);
            citation.CitationType = CitationTypes.Text;
        }

        private void AppendNextStartCharacter()
        {
            citation.LeadingSymbols.BumpEnd();
            citation.Label.BumpStart();
        }

        public StringRange GetBookRange(StringRange mainRange, IMarkedUpParagraph paragraph)
        {
            var citationRange = new StringRange();
            citationRange.Copy(mainRange);
            citationRange.MoveEndTo(paragraph.IndexOf(' ', mainRange.Start, mainRange.Length));
            return citationRange;
        }
        public static int GetBookIndex(StringRange range, IMarkedUpParagraph paragraph)
        {
            return TextReference.IndexOfBook(
                paragraph.StringAt(range));
        }
    }
}