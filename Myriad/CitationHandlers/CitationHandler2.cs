using System;
using System.Collections.Generic;
using Feliciana.Library;
using Myriad.Library;

namespace Myriad.CitationHandlers
{
    enum Modes { name = 0, chapter = 1, verse = 2, word = 3, start = 4 }
    public class CitationHandler2
    {

        StringRange rangeToParse;
        int[,] scriptureReference = new int[3, 5]
            { {-1,-1,-1,-1,-1 },
              {-1,-1,-1,-1,-1 },
              {-1,-1,-1,-1,-1 }
            };
        LabelTypes nameLength;
        int labelStart;
        int position;
        int startPosition;
        int continuation = 0;
        Modes mode;
        List<Citation> results;
        IParagraph paragraphToParse;
        int count;
        char token;
        public List<Citation> ParseCitations(StringRange givenRange, IParagraph givenParagraph)
        {
            try
            {
                InitializeParser(givenRange, givenParagraph);
                while (position <= rangeToParse.End)
                {
                    if (mode == Modes.start) SkipLeadingSpaces();
                    if (mode != Modes.name) startPosition = position;
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
            position = rangeToParse.Start;
            labelStart = rangeToParse.Start;
            count = Number.nothing;
            mode = Modes.start;
            nameLength = LabelTypes.Short;
        }

        public void SkipLeadingSpaces()
        {
            while ((position <= rangeToParse.End) && (paragraphToParse.CharAt(position) == ' '))
            {
                position++;
            }
        }

        public void GetCount()
        {
            bool foundZero = false;
            bool lookForNumber = true;
            count = Number.nothing;

            while (lookForNumber && (position <= rangeToParse.End))
            {
                char c = paragraphToParse.CharAt(position);
                switch (c)
                {
                    case '0':
                        foundZero = true;
                        count *= 10;
                        position++;
                        continue;
                    case '1':
                        count *= 10;
                        count++;
                        position++;
                        continue;
                    case '2':
                        count *= 10;
                        count += 2;
                        position++;
                        continue;
                    case '3':
                        count *= 10;
                        count += 3;
                        position++;
                        continue;
                    case '4':
                        count *= 10;
                        count += 4;
                        position++;
                        continue;
                    case '5':
                        count *= 10;
                        count += 5;
                        position++;
                        continue;
                    case '6':
                        count *= 10;
                        count += 6;
                        position++;
                        continue;
                    case '7':
                        count *= 10;
                        count += 7;
                        position++;
                        continue;
                    case '8':
                        count *= 10;
                        count += 8;
                        position++;
                        continue;
                    case '9':
                        count *= 10;
                        count += 9;
                        position++;
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
            while (position <= rangeToParse.End)
            {
                token = paragraphToParse.CharAt(position);
                if ((token == ' ') || (token == ':') ||
                    (token == ',') || (token == '-') ||
                    (token == '.') || (token == ';') ||
                    (token == '!') || (token == '–'))
                {
                    foundToken = true;
                    break;
                }
                if (position < rangeToParse.End) count = Result.notfound;
                position++;
            }
            if (!foundToken)
            {
                token = '`';
                position = rangeToParse.End;
            }
        }

        private bool EvaluateToken()
        {
            if (token == ' ') return SpaceToken();
            if (token == ':') return ColonToken();
            if ((token == ';') || (token=='`')) return SemiColonToken();
            if ((token == '-') || (token == '–')) return DashToken();
            if (token == ',') return CommaToken();
            if (token == '!') return BangToken();
            if (token == '.') return DotToken();
            return false;
        }

        private bool SpaceToken()
        {
            string currentName = paragraphToParse.StringAt(startPosition, position-1);
            count = IndexOfBook(currentName);
            if (count == Result.notfound)
            {
                if ((currentName == "First") || (currentName == "Second") ||
                    (currentName == "Third") || (currentName == "Song") ||
                    (currentName == "Song of"))
                {
                    count = Number.nothing;
                    position++;
                    mode = Modes.name;
                    return true;
                }
                if ((currentName == "1") || (currentName == "2") ||
                    (currentName == "3"))
                {
                    count = Number.nothing;
                    position++;
                    mode = Modes.name;
                    return true;
                }
                return false;
            }
            mode = Modes.chapter;
            ResetVerses();
            scriptureReference[0, (int)Modes.name] = count;
            nameLength = Bible.NameLength(currentName);
            position++;
            if (Bible.IsShortBook(count)) mode = Modes.verse;
            return true;
        }

        private bool ColonToken()
        {
            if (count == Result.notfound) return false;
            scriptureReference[continuation, (int)Modes.chapter] = count;
            mode = Modes.verse;
            position++;
            return true;
        }
        private bool SemiColonToken()
        {
            if (mode == Modes.start) return false;
            bool success = true;
            if ((mode == Modes.word) && (count == Result.notfound))
            {
                success = EvaluateWordStack();
                continuation = 0;
                position++;
                mode = Modes.start;
                nameLength = LabelTypes.Short;
                return success;
            }
            scriptureReference[continuation, (int)mode] = count;
            continuation = 0;
            position++;
            while (ReferenceToFirstVerseExists() && success)
            {
                success = EvaluateStack();
            }
            mode = Modes.start;
            nameLength = LabelTypes.Short;
            return success;
        }

        private bool DashToken()
        {
            if (continuation > 0)
            {
                ApplyShortCitation();
            }
            scriptureReference[Ordinals.first, (int)mode] = count;
            continuation = 1;
            position++;
            return true;
        }

        private bool CommaToken()
        {
            if ((mode== Modes.start) || (mode == Modes.name)) return false;
            scriptureReference[continuation, (int)mode] = count;
            position += 2;
            if (continuation == 0)
            {
                continuation = 2;
                return true;
            }
            return EvaluateStack();
        }

        private bool BangToken()
        {
            scriptureReference[continuation, (int)mode] = count;
            ApplyShortCitation();
            results[results.Count - 1].CitationType = CitationTypes.Verse;
            return false;
        }

        private bool DotToken()
        {
            if (mode != Modes.verse) return false;
            scriptureReference[continuation, (int)Modes.verse] = count;
            position++;
            mode = Modes.word;
            return true;
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
            if (scriptureReference[Ordinals.second, (int)mode] == Result.notfound)
            {
                ApplyShortCitation();
                ResetVerse(Ordinals.first);
                MoveVerse(Ordinals.third, Ordinals.first);
                return;
            }
            //apply long citation; reset verses
            ApplyLongCitation();
            continuation = 0;
            ResetVerse(Ordinals.first);
            ResetVerse(Ordinals.second);
            if (scriptureReference[Ordinals.second, (int)Modes.chapter] != Result.notfound)
            {
                scriptureReference[Ordinals.first, (int)Modes.chapter] = 
                    scriptureReference[Ordinals.second, (int)Modes.chapter];
                ResetVerse(Ordinals.second);
                scriptureReference[Ordinals.second, (int)Modes.chapter] = Result.notfound;
            }
            if (mode == Modes.word) mode = Modes.verse;
            if (scriptureReference[Ordinals.third, (int)Modes.verse] != Result.notfound)
            {
                MoveVerse(Ordinals.third, Ordinals.first);
            }
        }

        private void ApplyShortCitation()
        {
            if (Bible.IsShortBook(scriptureReference[Ordinals.first, (int)Modes.name]))
            {
                scriptureReference[Ordinals.first, (int)Modes.chapter] = 1;
            }
            VerseReference verseReference = new VerseReference(
                scriptureReference[Ordinals.first, (int)Modes.name],
                scriptureReference[Ordinals.first, (int)Modes.chapter],
                scriptureReference[Ordinals.first, (int)Modes.verse],
                scriptureReference[Ordinals.first, (int)Modes.word]);

            Citation citation = new Citation();
            citation.Set(verseReference);
            citation.LabelType = nameLength;
            citation.Label = new StringRange(labelStart, position-1);
            labelStart = position + 1;
            results.Add(citation);
        }

        private void ApplyLongCitation()
        {
            if (Bible.IsShortBook(scriptureReference[Ordinals.first, (int)Modes.name]))
            {
                scriptureReference[Ordinals.first, (int)Modes.chapter] = 1;
                scriptureReference[Ordinals.second, (int)Modes.chapter] = 1;
            }
            VerseReference firstReference = new VerseReference(
                scriptureReference[Ordinals.first, (int)Modes.name],
                scriptureReference[Ordinals.first, (int)Modes.chapter],
                scriptureReference[Ordinals.first, (int)Modes.verse],
                scriptureReference[Ordinals.first, (int)Modes.word]);
            VerseReference secondReference = new VerseReference(
                scriptureReference[Ordinals.second, (int)Modes.name],
                scriptureReference[Ordinals.second, (int)Modes.chapter],
                scriptureReference[Ordinals.second, (int)Modes.verse],
                scriptureReference[Ordinals.second, (int)Modes.word]);

            Citation citation = new Citation();
            citation.Set(firstReference, secondReference);
            citation.LabelType = nameLength;
            citation.Label = new StringRange(labelStart, position-1);
            labelStart = position + 1;
            results.Add(citation);
        }

        private bool EvaluateWordStack()
        {
            string currentWord = (token=='`') ? paragraphToParse.StringAt(startPosition, position) :
                paragraphToParse.StringAt(startPosition, position - 1);

            KeyID start = new KeyID(
                scriptureReference[Ordinals.first, (int)Modes.name],
                scriptureReference[Ordinals.first, (int)Modes.chapter],
                scriptureReference[Ordinals.first, (int)Modes.verse],
                currentWord);
            KeyID end = new KeyID(
                scriptureReference[Ordinals.first, (int)Modes.name],
                scriptureReference[Ordinals.first, (int)Modes.chapter],
                scriptureReference[Ordinals.first, (int)Modes.verse],
                KeyID.DeferredWordIndex);
            Citation citation = new Citation();
            citation = new Citation(start, end);
            citation.CitationType = CitationTypes.Verse;
            citation.Label = new StringRange(labelStart, position-1);
            labelStart = position + 1;
            results.Add(citation);
            return true;
        }

        private void ResetVerses()
        {
            for (int i = Ordinals.first; i <= Ordinals.third; i++)
            {
                ResetVerse(i);
            }
        }

        private void MoveVerse(int from, int to)
        {
            if (mode == Modes.chapter) 
                scriptureReference[to, (int)Modes.chapter] = scriptureReference[from, (int)Modes.chapter];
            scriptureReference[to, (int)Modes.verse] = scriptureReference[from, (int)Modes.verse];
            scriptureReference[to, (int)Modes.word] = scriptureReference[from, (int)Modes.word];
            ResetVerse(from); 
        }

        private void ResetVerse(int ordinal)
        {
            if (mode==Modes.chapter) scriptureReference[ordinal, (int)Modes.chapter] = Result.notfound;
            scriptureReference[ordinal, (int)Modes.verse] = Result.notfound;
            scriptureReference[ordinal, (int)Modes.word] = Result.notfound;
        }

        private void EvaluateThirdVerse()
        {
            if (scriptureReference[Ordinals.third, (int)mode] == scriptureReference[Ordinals.first, (int)mode] + 1)
            {
                MoveVerse(Ordinals.third, Ordinals.second);
            }  
        }


        private bool ReferenceToFirstVerseExists()
        {
            return scriptureReference[Ordinals.first, (int)mode] != Result.notfound;
        }

        protected virtual int IndexOfBook(string book)
        {
            return Bible.IndexOfBook(book);
        }
    }
}
