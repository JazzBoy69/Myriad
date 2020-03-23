﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Myriad.Library;

namespace Myriad.Parser

{
    public struct Tokens
    {
        public const int headingToken = 0x3D3D;
        public const int startSidenote = 0x2828;
        public const int endSidenote = 0x2929;
        public const int picture = 0x5B5B;
        public const int bold = 0x2A2A;
        public const int italic = 0x2F2F;
        public const int detail = 0x2B2B;
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
        abstract int IncreaseCitationLevel();
        abstract int DecreaseCitationLevel();
        abstract void HandleCitations();

    }
    public class BasicMarkupParser : IParser
    {
        protected int citationLevel = 0;
        protected IMarkedUpParagraph currentParagraph;
        protected StringRange mainRange = new StringRange();
        protected IMarkedUpParagraphCreator creator;
        protected bool foundEndToken;

        public IMarkedUpParagraph CurrentParagraph { get => currentParagraph; }
        public StringRange MainRange { get => mainRange;  }

        public void SetParagraphCreator(IMarkedUpParagraphCreator creator)
        {
            this.creator = creator;
        }
        public virtual void Parse(List<string> paragraphs)
        {
            foreach (string paragraph in paragraphs)
            {
                currentParagraph = creator.Create(paragraph);
                ParseParagraph();
            }

        }

        protected void ParseParagraph()
        {
            try
            {
                if (currentParagraph.Length == Numbers.nothing) return;
                Initialize();
                HandleStart();
                SearchForToken();
                foundEndToken = false;
                while (mainRange.Valid) 
                {
                    HandleToken();
                    if (foundEndToken) break;
                    MoveToNextToken();
                }
                if (!foundEndToken) HandleEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        protected void Initialize()
        {
            if (currentParagraph.Length == Numbers.nothing)
            {
                mainRange = StringRange.InvalidRange;
            }
            mainRange = new StringRange();
            mainRange.SetLimit(currentParagraph.Length - 1);
        }
        virtual protected void HandleStart()
        {

        }
        virtual protected void HandleEnd()
        {

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
            if (mainRange.End > mainRange.Max - 2) return;
            int longtoken = currentParagraph.TokenAt(mainRange.End);
            char token = currentParagraph.CharAt(mainRange.End);
            

            if (token == Tokens.endSidenote)
            {
                DecreaseCitationLevel();
                mainRange.BumpStart();
                mainRange.BumpStart();
                return;
            }
            if (longtoken == Tokens.startSidenote)
            {
                IncreaseCitationLevel();
                mainRange.BumpStart();
                mainRange.BumpStart();
                return;
            }
            if (longtoken == Tokens.picture)
            {
                return;
            }
            if (token == '{')
            {
                citationLevel = IncreaseCitationLevel();
                MoveIndexToNextBracketToken();
                mainRange.PullEnd();
                return;
            }
            if ((token == '(') || (token == '[') || (token == '~'))
            {
                citationLevel = IncreaseCitationLevel();
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

        public int IncreaseCitationLevel()
        {
            HandleCitations();
            citationLevel++;
            return citationLevel;
        }
        public int DecreaseCitationLevel()
        {
            HandleCitations();
            if (citationLevel > 0)
                citationLevel--;
            return citationLevel;
        }

        async private Task HandleCitationsAsync()
        {
            await Task.Run(() =>
            {
                HandleCitations();
            });
        }

        virtual public void HandleCitations()
        {
            //todo
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