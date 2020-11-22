using System;
using System.Collections.Generic;
using Feliciana.Library;
using Myriad.Library;

namespace Myriad.CitationHandlers
{
    public class CitationHandler2
    {
        const int name = 0;
        const int chapter = 1;
        const int verse = 2;
        const int word = 3;
        const int start = 4;
        Citation citation = new Citation();
        StringRange rangeToParse;
        StringRange labelRange;
        int[,] scriptureReference = new int[3, 4]
            { {-1,-1,-1,-1 },
              {-1,-1,-1,-1 },
              {-1,-1,-1,-1 }
            };
        LabelTypes nameLength;
        int continuation = 0;
        int mode;
        List<Citation> results;
        IParagraph paragraphToParse;
        int count;
        char token;
        public List<Citation> ParseCitations(StringRange givenRange, IParagraph givenParagraph)
        {
            try
            {
                InitializeParser(givenRange, givenParagraph);
                while (citation.Label.End <= rangeToParse.End)
                {
                    if (mode == start) SkipLeadingSpaces();
                    GetCount();
                    GetToken();
                    bool success = EvaluateToken();
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

        public void InitializeParser(StringRange givenRange, IParagraph givenParagraph)
        {
            rangeToParse = givenRange;
            paragraphToParse = givenParagraph;
            char lastChar = givenParagraph.CharAt(rangeToParse.End);
            if (lastChar == '.') rangeToParse.PullEnd();
            results = new List<Citation>();
            citation = new Citation();
            citation.Label.MoveStartTo(rangeToParse.Start);
            citation.Label.MoveEndTo(rangeToParse.Start);
            count = Number.nothing;
            mode = start;
            nameLength = LabelTypes.Short;
        }

        public void SkipLeadingSpaces()
        {
            citation.LeadingSymbols.MoveStartTo(citation.Label.Start);
            while ((citation.Label.Start <= rangeToParse.End) && (paragraphToParse.CharAt(citation.Label.Start) == ' '))
            {
                citation.Label.BumpStart();
            }
            citation.LeadingSymbols.MoveEndTo(citation.Label.Start - 1);
            citation.Label.MoveEndTo(citation.Label.Start);
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
                    foundToken = true;
                    break;
                }
                if (citation.Label.End < rangeToParse.End) count = Result.notfound;
                citation.Label.BumpEnd();
            }
            if (!foundToken)
            {
                token = ';';
            }
            if (citation.Label.End > rangeToParse.End)
                citation.Label.MoveEndTo(rangeToParse.End);
        }

        private bool EvaluateToken()
        {
            if (token == ' ') return SpaceToken();
            if (token == ':') return ColonToken();
            if (token == ';') return SemiColonToken();
            if ((token == '-') || (token == '–')) return DashToken();
            if (token == ',') return CommaToken();
            return false;
        }

        private bool SpaceToken()
        {
            string currentName = paragraphToParse.StringAt(citation.Label.Start, citation.Label.End - 1);
            count = IndexOfBook(currentName);
            if (count == Result.notfound)
            {
                if ((currentName == "First") || (currentName == "Second") ||
                    (currentName == "Third") || (currentName == "Song") ||
                    (currentName == "Song of"))
                {
                    count = Number.nothing;
                    citation.Label.BumpEnd();
                    mode = name;
                    nameLength = LabelTypes.Long;
                    return true;
                }
                if ((currentName == "1") || (currentName == "2") ||
                    (currentName == "3"))
                {
                    count = Number.nothing;
                    citation.Label.BumpEnd();
                    mode = name;
                    nameLength = LabelTypes.Normal;
                    return true;
                }
                return false;
            }
            scriptureReference[0, name] = count;
            citation.Label.BumpEnd();
            mode = (Bible.IsShortBook(count)) ? verse : chapter;
            return true;
        }

        private bool ColonToken()
        {
            if (count == Result.notfound) return false;
            scriptureReference[continuation, chapter] = count;
            mode = verse;
            citation.Label.BumpEnd();
            return true;
        }
        private bool SemiColonToken()
        {
            //Todo handle deferred word index
            scriptureReference[continuation, mode] = count;
            continuation = 0;
            citation.Label.BumpEnd();
            bool success = true;
            while (ReferenceToFirstVerseExists() && success)
            {
                success = EvaluateStack();
            }
            mode = start;
            return success;
        }

        private bool DashToken()
        {
            if (continuation > 0)
            {
                ApplyShortCitation();
            }
            scriptureReference[Ordinals.first, mode] = count;
            continuation = 1;
            citation.Label.BumpEnd();
            return true;
        }

        private bool CommaToken()
        {
            scriptureReference[continuation, mode] = count;
            citation.Label.BumpEnd();
            citation.Label.BumpEnd();
            if (continuation == 0)
            {
                continuation = 2;
                return true;
            }
            return EvaluateStack();
        }

        private bool EvaluateStack()
        {
            VerseReference firstVerse = new VerseReference();
            EvaluateThirdVerse();
            EvaluateSecondVerse();
            return true;
        }

        private void EvaluateSecondVerse()
        {
            if (scriptureReference[Ordinals.second, verse] == Result.notfound)
            {
                ApplyShortCitation();
                MoveVerse(Ordinals.third, Ordinals.first);
                return;
            }
            //apply long citation; reset verses
            ApplyLongCitation();
            continuation = 0;
            ResetVerse(Ordinals.first);
            ResetVerse(Ordinals.second);
            if (mode == word) mode = verse;
            if (scriptureReference[Ordinals.third, verse] != Result.notfound)
            {
                MoveVerse(Ordinals.third, Ordinals.first);
            }
        }

        private void ApplyShortCitation()
        {
            VerseReference verseReference = new VerseReference(
                scriptureReference[Ordinals.first, name],
                scriptureReference[Ordinals.first, chapter],
                scriptureReference[Ordinals.first, verse],
                scriptureReference[Ordinals.first, word]);

            Citation citation = new Citation();
            citation.Set(verseReference);
            citation.LabelType = nameLength;
            results.Add(citation);
        }

        private void ApplyLongCitation()
        {
            VerseReference firstReference = new VerseReference(
                scriptureReference[Ordinals.first, name],
                scriptureReference[Ordinals.first, chapter],
                scriptureReference[Ordinals.first, verse],
                scriptureReference[Ordinals.first, word]);
            VerseReference secondReference = new VerseReference(
                scriptureReference[Ordinals.second, name],
                scriptureReference[Ordinals.second, chapter],
                scriptureReference[Ordinals.second, verse],
                scriptureReference[Ordinals.second, word]);

            Citation citation = new Citation();
            citation.Set(firstReference, secondReference);
            citation.LabelType = nameLength;
            results.Add(citation);
        }

        private void ResetVerses()
        {
            for (int i = Ordinals.first; i <= Ordinals.third; i++)
            {
                ResetVerse(i);
            }
            nameLength = LabelTypes.Short;
        }

        private void MoveVerse(int from, int to)
        {
            scriptureReference[to, verse] = scriptureReference[from, verse];
            scriptureReference[to, word] = scriptureReference[from, word];
            ResetVerse(from); 
        }

        private void ResetVerse(int ordinal)
        {
            if (mode==chapter) scriptureReference[ordinal, chapter] = Result.notfound;
            scriptureReference[ordinal, verse] = Result.notfound;
            scriptureReference[ordinal, word] = Result.notfound;
        }

        private void EvaluateThirdVerse()
        {
            if (scriptureReference[Ordinals.third, verse] == scriptureReference[Ordinals.first, verse] + 1)
            {
                MoveVerse(Ordinals.third, Ordinals.second);
            }  
        }


        private bool ReferenceToFirstVerseExists()
        {
            return scriptureReference[Ordinals.first, mode] != Result.notfound;
        }

        protected virtual int IndexOfBook(string book)
        {
            return Bible.IndexOfBook(book);
        }
    }
}
