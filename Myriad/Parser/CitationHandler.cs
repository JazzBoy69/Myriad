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
                result.Add(citation);
                shouldContinue = false; //TODO: make loop
            }
            return result;
        }

        private Citation ParseCitation(StringRange mainRange, IMarkedUpParagraph paragraph)
        {
            citation = new Citation();
            citation.Label = GetBookRange(mainRange, paragraph);
            int book = GetBookIndex(citation.Label, paragraph);
            if (book == TextReference.invalidBook) return Citation.InvalidCitation;

            int mode = TextReference.IsShortBook(book) ? findVerse : findChapter;
            int count = 0;
            int chapter = TextReference.invalidChapter;
            bool foundZero = false;
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
            return citation;
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