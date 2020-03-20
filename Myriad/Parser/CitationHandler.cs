using System;
using System.Collections.Generic;
using Myriad.Library;


namespace Myriad.Parser
{
    public class CitationHandler
    {
        int count = 0;
        char token;
        char lastToken;
        char tokenBeforeLast;
        Citation citation = new Citation();
        VerseReference firstVerse = new VerseReference();
        VerseReference secondVerse = new VerseReference();
        List<Citation> results;
        StringRange rangeToParse;
        StringRange labelRange;
        IMarkedUpParagraph paragraphToParse;
        bool first = true;
        int commaAt = Result.notfound;

        public Citation Citation
        {
            get
            {
                return citation;
            }
        }

        public VerseReference FirstVerse
        {
            get
            {
                return firstVerse;
            }
        }

        public List<Citation> Results
        {
            get
            {
                return results;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }
        public List<Citation> ParseCitations(StringRange givenRange, IMarkedUpParagraph givenParagraph)
        {
            InitializeParser(givenRange, givenParagraph);
            while (citation.Label.End <= rangeToParse.End)
            {
                if (first) SkipLeadingSymbols();
                GetCount();
                GetToken();
                if (count == Result.notfound)
                {
                    if (FoundBook())
                        continue;
                    else
                        break;
                }
                if ((lastToken == ' ') && (token == ':'))
                {
                    firstVerse.Chapter = count;
                    MoveToNext();
                    continue;
                }
                if ((lastToken == ' ') && (token == ';'))
                {
                    firstVerse.Chapter = count;
                    AddCitationToResults();
                    continue;
                }
                if ((lastToken == '-') && (token == ':'))
                {
                    secondVerse.Chapter = count;
                    MoveToNext();
                    continue;
                }
                if (LookingForFirstVerse() && (token == ','))
                {
                    firstVerse.Verse = count;
                    commaAt = citation.Label.End;
                    citation.Label.BumpEnd();
                    MoveToNext();
                    continue;
                }
                if ((tokenBeforeLast == '-') && (lastToken == ':') && (token == ','))
                { 
                    secondVerse.Verse = count;
                    MoveToNext();
                    lastToken = ';';
                    continue;
                }
                if (((lastToken == '-') || (lastToken == ',')) && (token == ';'))
                {
                    secondVerse.Verse = count;
                    AddCitationToResults();
                    continue;
                }
                if ((lastToken == '-') || (lastToken == ','))
                {
                    secondVerse.Verse = count;
                    MoveToNext();
                    continue;
                }
                if (LookingForFirstVerse() && (token == ';'))
                {
                    firstVerse.Verse = count;
                    AddCitationToResults();
                    continue;
                }
                if (token == ';')
                {
                    secondVerse.Verse = count;
                    AddCitationToResults();
                    continue;
                }
                break;
            }
            return results;
        }

        public bool FoundBook()
        {
            labelRange.MoveEndTo(citation.Label.End);
            if ((lastToken == ';') && (token == ' '))
            {
                firstVerse.Book = Bible.IndexOfBook(paragraphToParse.StringAt(labelRange));
                MoveToNext();
                return true;
            }
            if ((lastToken == '-') && (token == ' '))
            {
                secondVerse.Book = Bible.IndexOfBook(paragraphToParse.StringAt(labelRange));
                MoveToNext();
                return true;
            }
            return false;
        }

        public void GetToken()
        {
            bool foundToken = false;
            while (citation.Label.End <= rangeToParse.End)
            {
                token = paragraphToParse.CharAt(citation.Label.End);  //Todo: add !
                if ((token == ' ') || (token == ':') ||
                    (token == ',') || (token == '-') ||
                    (token == '.') || (token == ';'))
                {
                    foundToken = true;
                    break;
                }
                if (citation.Label.End<rangeToParse.End) count = Result.notfound;
                citation.Label.BumpEnd();
            }
            if (!foundToken)
            {
                token = ';';
            }
            if (citation.Label.End > rangeToParse.End)
                citation.Label.MoveEndTo(rangeToParse.End);
        }

        public void GetCount()
        {
            labelRange = new StringRange(citation.Label.Start, citation.Label.Start);
            bool foundZero = false;
            bool lookForNumber = true;
            count = Numbers.nothing;
            
            while (lookForNumber && (citation.Label.End <= rangeToParse.End))
            {
                char c = paragraphToParse.CharAt(citation.Label.End);
                switch (c)
                {
                    case '0':
                        foundZero = true;
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
                        lookForNumber = false;
                        break;
                }
                if ((count == 0) && (!foundZero)) count = Result.notfound;
            }
        }

        public void SkipLeadingSymbols()
        {
            while ((citation.Label.End <= rangeToParse.End) && (paragraphToParse.CharAt(citation.Label.Start) == ' '))
            {
                citation.LeadingSymbols.BumpEnd();
                citation.Label.BumpStart();
            }
        }

        public void InitializeParser(StringRange givenRange, IMarkedUpParagraph givenParagraph)
        {
            rangeToParse = givenRange;
            paragraphToParse = givenParagraph;
            results = new List<Citation>();
            citation = new Citation();
            citation.Label.MoveStartTo(rangeToParse.Start);
            citation.Label.MoveEndTo(rangeToParse.Start);
            count = Numbers.nothing;
            firstVerse.Reset();
            secondVerse.Reset();
            tokenBeforeLast = ';';
            lastToken = ';';
        }

        private void AddCitationToResults()
        {
            
            if (secondVerse.WordIndex != Result.notfound)
            {
                citation.CitationRange.Set(firstVerse.Book, firstVerse.Chapter, firstVerse.Verse,
                    firstVerse.WordIndex, secondVerse.Chapter, secondVerse.Verse,
                    secondVerse.WordIndex);

                citation.CitationType = CitationTypes.Text;
                Reset();
                return;
            }
            if (secondVerse.Verse != Result.notfound)
            {
                if (firstVerse.WordIndex != Result.notfound)
                {
                    citation.CitationRange.Set(firstVerse.Book, firstVerse.Chapter, firstVerse.Verse,
                     firstVerse.WordIndex, secondVerse.Chapter, secondVerse.Verse,
                     KeyID.MaxWordIndex);
                }
                else
                {
                    citation.CitationRange.Set(firstVerse.Book, firstVerse.Chapter, firstVerse.Verse,
                     secondVerse.Chapter, secondVerse.Verse);
                }
                citation.CitationType = CitationTypes.Text;
                Reset();
                return;
            }
            if (secondVerse.Chapter != Result.notfound)
            {
                citation.CitationRange.Set(firstVerse.Book, firstVerse.Chapter, Ordinals.first,
                    secondVerse.Chapter, Bible.Chapters[firstVerse.Book][secondVerse.Chapter]);
                citation.CitationType = CitationTypes.Text;
                Reset();
                return;
            }
            if (firstVerse.WordIndex != Result.notfound)
            {
                citation.CitationRange.Set(firstVerse.Book, firstVerse.Chapter, firstVerse.Verse,
                    firstVerse.WordIndex);
                citation.CitationType = CitationTypes.Text;
                Reset();
                return;
            }
            if (firstVerse.Verse != Result.notfound)
            {
                citation.CitationRange.Set(firstVerse.Book, firstVerse.Chapter, firstVerse.Verse);
                citation.CitationType = CitationTypes.Text; 
                Reset();
                return;
            }
            if (firstVerse.Chapter != Result.notfound)
            {
                citation.CitationRange.Set(firstVerse.Book, firstVerse.Chapter);
                citation.CitationType = CitationTypes.Chapter;
                Reset();
                return;
            }
            EndParsingSession();
        }

        public void SetFirstVerse()
        {
            firstVerse.Verse = count;
        }

        public void SetFirstChapter()
        {
            firstVerse.Chapter = count;
        }

        private void EndParsingSession()
        {
            citation.LeadingSymbols.MoveEndTo(rangeToParse.End + 1);
            citation.Label.MoveEndTo(rangeToParse.End + 1);
        }

        private void Reset()
        {
            int pointer = citation.Label.End;
            if (pointer < rangeToParse.End)
            {
                citation.TrailingSymbols.MoveStartTo(pointer);
                citation.TrailingSymbols.MoveEndTo(pointer);
                citation.Label.PullEnd();
                rangeToParse.MoveStartTo(pointer);
                pointer++;
            }
            Citation newCitation = citation.Copy();
            results.Add(newCitation);
            citation = new Citation();
            pointer++;
            citation.LeadingSymbols.MoveStartTo(pointer);
            citation.LeadingSymbols.MoveEndTo(pointer);
            citation.Label.MoveStartTo(pointer);
            citation.Label.MoveEndTo(pointer);
            first = true;
        }

        private bool LookingForFirstVerse()
        {
            return ((tokenBeforeLast == ' ') || (tokenBeforeLast == ';'))
                    && (lastToken == ':');
        }


        public void MoveToNext()
        {
            tokenBeforeLast = lastToken;
            lastToken = token;
            count = Numbers.nothing;
            citation.Label.BumpEnd();
            first = false;
        }

        public static int GetBookIndex(StringRange range, IMarkedUpParagraph paragraph)
        {
            return TextReference.IndexOfBook(
                paragraph.StringAt(range));
        }
    }
}