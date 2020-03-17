using System;
using System.Collections.Generic;
using Myriad.Library;

namespace Myriad.Parser

{
    public struct Tokens
    {
        public static readonly char[] citationTokens = new char[] { '(', '[', '{', ')', ']', '}' };
        public static readonly char[] brackettokens = new char[] { '|', '}' };
        public static readonly char[] tokens = new char[] { '*', '^', '/', '=', '(', '[', '{', ')', ']', '}', '~', '#', '|', '_', '+' };
    }
    public interface IParser
    {
        abstract void Parse(List<string> paragraphs);

        IMarkedUpParagraph CurrentParagraph { get; }

        StringRange MainRange { get; }

        abstract void SetParagraphCreator(IMarkedUpParagraphCreator creator);
       
        abstract void SearchForToken();
        abstract void HandleToken();
        abstract int IncreaseCitationLevel(char token);
        abstract int DecreaseCitationLevel();
        abstract void HandleCitations();

    }
    public class BasicMarkupParser : IParser
    {
        protected int citationLevel = 0;
        protected IMarkedUpParagraph currentParagraph;
        protected StringRange mainRange = new StringRange();
        protected IMarkedUpParagraphCreator creator;

        public IMarkedUpParagraph CurrentParagraph { get => currentParagraph; }
        public StringRange MainRange { get => mainRange;  }

        public void SetParagraphCreator(IMarkedUpParagraphCreator creator)
        {
            this.creator = creator;
        }
        public void Parse(List<string> paragraphs)
        {
            foreach (string paragraph in paragraphs)
            {
                currentParagraph = creator.Create(paragraph);
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
            HandleEnd();
        }

        virtual protected void HandleEnd()
        {

        }

        protected void MoveToFirstToken()
        {
            if (currentParagraph.Length == Numbers.nothing)
            {
                mainRange = StringRange.InvalidRange;
            }
            mainRange = new StringRange();
            mainRange.SetLimit(currentParagraph.Length - 1);
            SearchForToken();
        }

        protected void MoveToNextToken()
        {
            if (mainRange.Start > currentParagraph.Length)
                return;
            int end = mainRange.End;
            SearchForToken();
            if (mainRange.End == end)
            {
                mainRange.BumpStart();
                MoveToNextToken();
            }
        }

        virtual public void SearchForToken()
        {
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(Tokens.citationTokens, mainRange.Start));
        }

        virtual public void HandleToken()
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
            //TODO
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
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(Tokens.brackettokens, mainRange.Start));
            if (mainRange.End == end)
            {
                mainRange.BumpStart();
                MoveIndexToNextBracketToken();
            }
        }
    }
}