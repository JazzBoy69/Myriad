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
        CitedVerse verse = new CitedVerse();
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
        public List<Citation> ParseCitations(StringRange givenRange, IMarkedUpParagraph givenParagraph)
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

        private bool TakeAction()
        {
            int action = TokenDictionary.Lookup(tokenBeforeLast, lastToken, token, count);
            if (action == Result.notfound) return false;
            string book = paragraphToParse.StringAt(citation.Label.Start, citation.Label.End-1); //todo remove
            if (count == Result.notfound) count =
                    Bible.IndexOfBook(paragraphToParse.StringAt(citation.Label.Start, citation.Label.End-1));
            if (count == Result.notfound) return false;
            verse.Set(action & 7, count);
            if (action > 0xF)
            {
                AddCitationToResults();
                if (citation.CitationType == CitationTypes.Invalid)
                {
                    EndParsingSession();
                    return true;
                }
                Reset();
            }
            else
                MoveToNext();
            return true;
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
                    (token == '!'))
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
            verse.First.Reset();
            verse.Second.Reset();
            tokenBeforeLast = ';';
            lastToken = ';';
        }

        private void AddCitationToResults()
        {
            int stashVerse = Result.notfound;
            if ((verse.Second.Verse != Result.notfound) &&
                ((lastToken == ',') && (verse.First.Verse + 1 != verse.Second.Verse)))
             {
                if (verse.First.Verse == verse.Second.Verse)
                {
                    lastToken = ';';
                }
                stashVerse = verse.Second.Verse;
                verse.Second.Reset();
            }
            citation.Set(verse.First, verse.Second);
            if ((token == '!') && (citation.CitationType == CitationTypes.Text))
                citation.CitationType = CitationTypes.Verse;
            if (stashVerse != Result.notfound)
            {
                verse.Second.Verse = stashVerse;
            }
        }

        private void EndParsingSession()
        {
            citation.LeadingSymbols.MoveEndTo(rangeToParse.End + 1);
            citation.Label.MoveEndTo(rangeToParse.End + 1);
        }

        private void Reset()
        {
            int pointer = citation.Label.End;
            if ((lastToken == ',') && (verse.First.Verse + 1 != verse.Second.Verse))
            {
                verse.First.Verse = verse.Second.Verse;
                verse.Second.Reset();
                pointer = commaAt;
                citation.Label.MoveEndTo(commaAt+1);
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
                rangeToParse.MoveStartTo(pointer);
                pointer++;
            }
            Citation newCitation = citation.Copy();
            results.Add(newCitation);
            citation = new Citation();
            pointer++;
            //if (lastToken == ',') pointer++;
            citation.LeadingSymbols.MoveStartTo(pointer);
            citation.LeadingSymbols.MoveEndTo(pointer);
            citation.Label.MoveStartTo(pointer);
            citation.Label.MoveEndTo(pointer);
            first = true;
        }

        public void MoveToNext()
        {
            tokenBeforeLast = lastToken;
            lastToken = token;
            count = Numbers.nothing;
            citation.Label.BumpEnd();
            if (lastToken == ',')
            {
                commaAt = citation.Label.End-1;
                citation.Label.BumpEnd();
            }
            first = false;
        }
    }
}