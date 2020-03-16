using System;
using System.Collections.Generic;
using Myriad.Library;
using Range = Myriad.Library.Range;


namespace Myriad.Parser

{
    public class BasicMarkupParser
    {
        protected int citationLevel = 0;
        protected MarkedUpParagraph currentParagraph;
        protected StringRange mainRange;
        static readonly char[] tokens = new char[] {'(', '[', '{', ')', ']', '}'};
        static readonly char[] brackettokens = new char[] { '|', '}' };

        public void Parse(List<MarkedUpParagraph> paragraphs)
        {
            foreach (MarkedUpParagraph paragraph in paragraphs)
            {
                currentParagraph = paragraph;
                ParseParagraph();
            }
        }

        protected void ParseParagraph()
        {
            MoveToFirstToken();
            while (mainRange.Valid)
            {
                HandleToken();
                MoveToNextToken();
            }
        }

        private void MoveToFirstToken()
        {
            if (currentParagraph.Length == Numbers.nothing)
            {
                mainRange = StringRange.InvalidRange;
            }
            mainRange = new StringRange();
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(tokens, mainRange.Start));
        }

        internal void MoveToNextToken()
        {
            if (mainRange.Start > currentParagraph.Length)
                return;
            int end = mainRange.End;
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(tokens, mainRange.Start));
            if (mainRange.End == end)
            {
                mainRange.BumpStart();
                MoveToNextToken();
            }
        }
        protected void HandleToken()
        {
            char token = currentParagraph.CharAt(mainRange.End);
            char charAfterToken = currentParagraph.CharAt(mainRange.End + 1);

            if ((token == ')') && (charAfterToken == ')'))
            {
                mainRange.BumpStart();
                mainRange.BumpStart();
                return;
            }
            if ((token == '(') && (charAfterToken == '('))
            {
                mainRange.BumpStart();
                mainRange.BumpStart();
                return;
            }
            if ((token == '[') && (charAfterToken == '['))
            {
                return;
            }
            if ((token == '(') || (token == '[') || (token == '{') || (token == '~'))
            {
                citationLevel = IncreaseCitationLevel(token);
                return;
            }
            if ((token == ')') || (token == ']') || (token == '}'))
            {
                citationLevel = DecreaseCitationLevel();
                return;
            }
        }

        protected void MoveIndexToEndBracket()
        {
            if (mainRange.Start > currentParagraph.Length)
                return;
            mainRange.MoveEndTo(currentParagraph.IndexOf('}', mainRange.Start - 1));
        }

        protected void MoveIndexToEndOfWord()
        {
            mainRange.BumpEnd();
            while ((!mainRange.AtLimit) &&
                (Symbols.IsPartOfWord(currentParagraph.CharAt(mainRange.End))))
                mainRange.BumpEnd();
            mainRange.PullEnd();
        }

        protected int IncreaseCitationLevel(char token)
        {
            HandleCitations();
            citationLevel++;
            if (token == '{')
            {
                MoveIndexToNextBracketToken();
                mainRange.PullEnd();
            }
            return citationLevel;
        }
        protected int DecreaseCitationLevel()
        {
            HandleCitations();
            if (citationLevel > 0)
                citationLevel--;
            return citationLevel;
        }

        protected void HandleCitations()
        {
            throw new NotImplementedException();
        }

        protected void ResetCitationLevel()
        {
            citationLevel = 0;
        }

        internal void MoveIndexToNextBracketToken()
        {
            if (mainRange.Start > currentParagraph.Length)
                return;
            int end = mainRange.End;
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(brackettokens, mainRange.Start));
            if (mainRange.End == end)
            {
                mainRange.BumpStart();
                MoveIndexToNextBracketToken();
            }
        }
    }
}