using System;
using System.Collections.Generic;
using Feliciana.Library;
using Myriad.Library;
using Myriad.CitationHandlers.Helpers;



namespace Myriad.CitationHandlers
{
    public class CitationHandler
    {
        int count = 0;
        char token;
        int tokenAt;
        char lastToken;
        int lastTokenAt = Result.notfound;
        char tokenBeforeLast;
        Citation citation = new Citation();
        readonly CitedVerse verse = new CitedVerse();
        List<Citation> results;
        StringRange rangeToParse;
        StringRange labelRange;
        IParagraph paragraphToParse;
        bool first = true;
        bool brokenComma = false;
        int currentBook;
        int currentChapter;
        string currentWord;

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
                return verse.First;
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
        public List<Citation> ParseCitations(StringRange givenRange, IParagraph givenParagraph)
        {
            try
            {
                InitializeParser(givenRange, givenParagraph);
                while (citation.Label.End <= rangeToParse.End)
                {
                    if (first) SkipLeadingSymbols();
                    GetCount();
                    GetToken();
                    bool success = TakeAction();
                    if (!success) break;
                }
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        private bool TakeAction()
        {
            if (((count > 0) && (count < 4)) && (token == ' '))
            {
                MoveToNext();
                return true;
            }
            int action = TokenDictionary.Lookup(tokenBeforeLast, lastToken, token, count);
            if (action == Result.notfound) 
                return false;
            if (action == TokenDictionary.DeferWordIndex)
            {
                count = KeyID.DeferredWordIndex;
                currentWord = paragraphToParse.StringAt(lastTokenAt+1, citation.Label.End);
            }
            if (count == Result.notfound)
            {
                currentWord = paragraphToParse.StringAt(citation.Label.Start, citation.Label.End - 1);
                count = IndexOfBook(currentWord);
                if (count == Result.notfound)
                {
                    if ((currentWord == "First") || (currentWord == "Second") || 
                        (currentWord == "Third") || (currentWord == "Song") || 
                        (currentWord == "Song of"))
                    {
                        count = Number.nothing;
                        citation.Label.BumpEnd();
                        first = false;
                        return true;
                    }
                }
            }
            if (count == Result.notfound) return false;

            if (action < TokenDictionary.DeferWordIndex)
            {
                verse.Set(action & 7, count);
            }
            if (action > 0xF)
            {
                ApplyVerseToCitation();
                if (citation.CitationType == CitationTypes.Invalid)
                {
                    EndParsingSession();
                    return true;
                }
                AddCitationToResults();
                if (action == TokenDictionary.AddBrokenCommaMarker)
                {
                    tokenBeforeLast = '~';
                    verse.First.Book = currentBook;
                    verse.First.Chapter = currentChapter;
                    verse.First.Verse = count;
                }
                count = Number.nothing;
            }
            else
                MoveToNext();
            return true;
        }

        protected virtual int IndexOfBook(string book)
        {
            return Bible.IndexOfBook(book);
        }

        public void GetToken()
        {
            bool foundToken = false;
            while (citation.Label.End <= rangeToParse.End)
            {
                token = paragraphToParse.CharAt(citation.Label.End);
                if ((token == ' ') || (token == ':') ||
                    (token == ',') || (token == '-') ||
                    (token == '.') || (token == ';') ||
                    (token == '!') || (token == '–'))
                {
                    lastTokenAt = tokenAt;
                    tokenAt = citation.Label.End;
                    foundToken = true;
                    break;
                }
                if (citation.Label.End<rangeToParse.End) count = Result.notfound;
                citation.Label.BumpEnd();
            }
            if (!foundToken)
            {
                token = ';';
                lastTokenAt = tokenAt;
                tokenAt = rangeToParse.End;
            }
            if (citation.Label.End > rangeToParse.End)
                citation.Label.MoveEndTo(rangeToParse.End);
        }

        public void GetCount()
        {
            labelRange = new StringRange(citation.Label.Start, citation.Label.Start);
            bool foundZero = false;
            bool lookForNumber = true;
            count = Number.nothing;
            
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
            citation.LeadingSymbols.MoveStartTo(citation.Label.Start);
            while ((citation.Label.Start <= rangeToParse.End) && (paragraphToParse.CharAt(citation.Label.Start) == ' '))
            {
                citation.Label.BumpStart();
            }
            citation.LeadingSymbols.MoveEndTo(citation.Label.Start - 1);
            citation.Label.MoveEndTo(citation.Label.Start);
        }

        public void InitializeParser(StringRange givenRange, IParagraph givenParagraph)
        {
            currentBook = Result.notfound;
            currentChapter = Result.notfound;
            currentWord = "";
            rangeToParse = givenRange;
            paragraphToParse = givenParagraph;
            char lastChar = givenParagraph.CharAt(rangeToParse.End);
            if (lastChar == '.') rangeToParse.PullEnd();
            results = new List<Citation>();
            citation = new Citation();
            citation.Label.MoveStartTo(rangeToParse.Start);
            citation.Label.MoveEndTo(rangeToParse.Start);
            count = Number.nothing;
            verse.First.Reset();
            verse.Second.Reset();
            tokenBeforeLast = ';';
            lastToken = ';';
        }

        private void ApplyVerseToCitation()
        {
            int stashVerse = Result.notfound;
            brokenComma = false;
            if ((verse.Second.Verse != Result.notfound) &&
                (verse.First.Book == Result.notfound) &&
                (verse.First.Chapter == Result.notfound) &&
                (verse.First.Verse == Result.notfound))
            {
                verse.First.Book = currentBook;
                verse.First.Chapter = currentChapter;
                verse.First.Verse = verse.Second.Verse;
            }
            if ((verse.First.Book == Result.notfound) && (verse.First.Chapter != Result.notfound))
            {
                verse.First.Book = currentBook;
            }
            if ((verse.Second.Verse != Result.notfound) &&
                ((lastToken == ',') && (Math.Abs(verse.Second.Verse - verse.First.Verse) > 1)))
            {
                if (verse.First.Verse == verse.Second.Verse)
                {
                    lastToken = '~';
                }
                stashVerse = verse.Second.Verse;
                brokenComma = true;
                verse.Second.Reset();
            }
            if ((verse.First.Verse != Result.notfound) && (results.Count > Number.nothing))
            {
                if (verse.First.Chapter == Result.notfound)
                {
                    verse.First.Chapter = results[Ordinals.last].CitationRange.LastChapter;
                }
                if (verse.First.Book == Result.notfound)
                {
                    verse.First.Book = results[Ordinals.last].CitationRange.Book;
                }
            }
            if (count == KeyID.DeferredWordIndex)
            {
                KeyID start = new KeyID(verse.First.Book, verse.First.Chapter,
                    verse.First.Verse, currentWord);
                KeyID end = new KeyID(verse.First.Book, verse.First.Chapter,
                    verse.First.Verse, KeyID.DeferredWordIndex);
                citation.CitationType = CitationTypes.Verse;
                citation.CitationRange = new CitationRange(start, end);
                return;
            }
            if ((token == ',') && (verse.First.Verse == Result.notfound) && (verse.Second.Chapter != Result.notfound)
                && (verse.Second.Verse == Result.notfound))
            {
                citation.Set(verse.First);
                VerseReference second = new VerseReference();
                second.Book = verse.First.Book;
                second.Chapter = verse.Second.Chapter;
                brokenComma = true;
                int end = citation.Label.End;
                AddCitationToResults();
                int start = citation.Label.End + 1;
                verse.First = second;
                brokenComma = false;
                citation.Set(verse.First);
                citation.Label.MoveEndTo(end);
                citation.Label.MoveStartTo(start);
                lastToken = ' ';
                token = '~';
            }
            else
            {
                if ((token == ',') && (verse.First.Verse == Result.notfound))
                {
                    lastToken = ' ';
                    token = '~';
                }
                citation.Set(verse.First, verse.Second);
            }
            currentBook = citation.CitationRange.Book;
            currentChapter = citation.CitationRange.LastChapter;
            if ((token == '!') && (citation.CitationType == CitationTypes.Text))
                citation.CitationType = CitationTypes.Verse;
            if (stashVerse != Result.notfound)
            {
                verse.First.Verse = stashVerse;
                verse.Second.Reset();
            }
        }

        private void EndParsingSession()
        {
            citation.LeadingSymbols.MoveEndTo(rangeToParse.End + 1);
            citation.Label.MoveEndTo(rangeToParse.End + 1);
        }

        private void AddCitationToResults()
        {
            int pointer = citation.Label.End;
            if (brokenComma)
            {
                pointer = lastTokenAt;
                citation.Label.MoveEndTo(lastTokenAt);
                citation.TrailingSymbols.MoveStartTo(lastTokenAt + 1);
                citation.TrailingSymbols.MoveEndTo(lastTokenAt + 1);
                lastToken = '~';
            }
            else
            {
                verse.First.Reset();
                verse.Second.Reset();
            }
            if (pointer < rangeToParse.End)
            {
                citation.TrailingSymbols.MoveStartTo(pointer);
                citation.TrailingSymbols.MoveEndTo(pointer);
                citation.Label.PullEnd();
            }
            pointer++;
            var newCitation = citation.Copy();
            if (newCitation.CitationRange.Valid)
                results.Add(newCitation);
            citation = new Citation();
            citation.Label.MoveStartTo(pointer);
            citation.Label.MoveEndTo(pointer);
            first = true;
            if (!brokenComma)
            {
                tokenBeforeLast = lastToken;
                lastToken = token;
            }
        }

        public void MoveToNext()
        {
            tokenBeforeLast = lastToken;
            lastToken = token;
            count = Number.nothing;
            citation.Label.BumpEnd();
            if (lastToken == ',')
            {
                citation.Label.BumpEnd();
            }
            first = false;
        }
    }
}