using System;
using System.Collections.Generic;
using Myriad.Library;

namespace Myriad.Parser

{
    public interface IParser<T>
    {
        abstract void Parse(List<T> paragraphs);

        abstract void MoveToFirstToken();
        abstract void MoveToNextToken();
        abstract void HandleToken();
        abstract int IncreaseCitationLevel(char token);
        abstract int DecreaseCitationLevel();
        abstract void HandleCitations();

    }
    public class BasicMarkupParser<T> : IParser<T> where T: MarkedUpParagraph
    {
        protected int citationLevel = 0;
        protected T currentParagraph;
        protected StringRange mainRange;
        protected static readonly char[] tokens = new char[] {'(', '[', '{', ')', ']', '}'};
        static readonly char[] brackettokens = new char[] { '|', '}' };

        public void Parse(List<T> paragraphs)
        {
            foreach (T paragraph in paragraphs)
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

        public void MoveToFirstToken()
        {
            if (currentParagraph.Length == Numbers.nothing)
            {
                mainRange = StringRange.InvalidRange;
            }
            mainRange = new StringRange();
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(tokens, mainRange.Start));
        }

        public void MoveToNextToken()
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
        public void HandleToken()
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

        public int IncreaseCitationLevel(char token)
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
        public int DecreaseCitationLevel()
        {
            HandleCitations();
            if (citationLevel > 0)
                citationLevel--;
            return citationLevel;
        }

        public void HandleCitations()
        {
            throw new NotImplementedException();
        }

        protected void ResetCitationLevel()
        {
            citationLevel = 0;
        }

        protected void MoveIndexToNextBracketToken()
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