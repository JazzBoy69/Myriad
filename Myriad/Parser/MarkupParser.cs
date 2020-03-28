﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Myriad.Library;

namespace Myriad.Parser
{
    internal class Formats
    {
        internal bool detail = false;
        internal bool bold = false;
        internal bool super = false;
        internal bool italic = false;
        internal bool heading = false;
        internal bool editable = true;
        internal bool figure = false;
        internal bool labelExists = false;
        internal bool hideDetails = false;
        internal bool sidenote = false;
        internal void Reset()
        {
            detail = false;
            bold = false;
            super = false;
            italic = false;
            heading = false;
            editable = true;
            figure = false;
            labelExists = false;
            hideDetails = false;
            sidenote = false;
        }
    }
    public class MarkupParser : BasicMarkupParser
    {
        internal readonly Formats formats = new Formats();
        readonly PageFormatter formatter;
        readonly CitationHandler citationHandler;
        readonly StringRange labelRange = new StringRange();


        public MarkupParser(HTMLResponse builder)
        {
            formatter = new PageFormatter(this, builder);
            citationHandler = new CitationHandler();
        }

        public string ParsedText { get { return formatter.Result; } }

        protected override void HandleStart()
        {
            formats.Reset();
            formatter.StartSection();
            if (currentParagraph.Length > 1)
            {
                HandleStartToken();
            }
            if (!formats.heading && !formats.figure)
            {
                formatter.StartParagraph();
            }

        }

        protected bool HandleStartToken()
        {
            int token = currentParagraph.TokenAt(Ordinals.first);
            if (token == Tokens.headingToken)
            {
                formats.editable = false;
                formats.heading = true;
                formatter.StartHeading(); 
                mainRange.BumpStart();
                mainRange.BumpStart();
                return false;
            }

            if (token == Tokens.startSidenote)
            {
                formats.editable = false;
                formats.sidenote = true;
                if (currentParagraph.Length > 2)
                    formatter.StartSidenoteWithHeading(formats);
                else
                    formatter.StartSidenote(formats);
                SkipLongToken();
                return false;
            }
            if (token == Tokens.picture)
            {
                formats.editable = false;
                formats.figure = true;
                formatter.AppendFigure(currentParagraph.Text, formats);
                return true;
            }
            //Todo Handle Table tokens

            return false;
        }

        protected override void HandleEnd()
        {
            mainRange.MoveEndToLimit();
            mainRange.MoveStartTo(mainRange.Start - 1);
            formatter.AppendString(currentParagraph, mainRange);
            if (formats.heading)
                formatter.EndHeading();
            else
                formatter.EndParagraph();
            if (!formats.sidenote)
            {
                formatter.EndSection();
                formatter.AppendClearDiv();
            }
        }

        protected void ParseMainHeading()
        {
            formatter.StartMainHeading();
            formatter.AppendString(currentParagraph, Ordinals.third, currentParagraph.Length - 3);
            formatter.EndMainHeading();
            formatter.StartComments();
        }

        protected void EndComments()
        {
            formatter.EndComments();
        }

        public override void SearchForToken()
        {
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(Tokens.tokens, mainRange.Start));
        }
        override public void HandleToken()
        {
            char token = currentParagraph.CharAt(mainRange.End);
            int longToken = currentParagraph.TokenAt(mainRange.End);
            if (token == '|')
            {
                if (citationLevel > 0)
                    SetLabel();
                SkipToken();
                return;
            }
            if (citationLevel > Numbers.nothing)
                HandleCitations();
            AppendTextUpToToken();
            if (longToken == Tokens.detail)
            {
                formats.detail = formatter.HandleDetails(formats.detail, formats);
                SkipLongToken();
                return;
            }

            if (longToken == Tokens.bold)
            {
                formats.bold = formatter.ToggleBold(formats.bold);
                SkipLongToken();
                return;
            }
            if (token == '^')
            {
                formats.super = formatter.ToggleSuperscription(formats.super);
                SkipToken();
                return;
            }
            if (longToken == Tokens.italic)
            {
                formats.italic = formatter.ToggleItalic(formats.italic);
                SkipLongToken();
                return;
            }
            if (token == '/')
            {
                AppendToken();
                return;
            }
            if (longToken == Tokens.endSidenote)
            {
                formatter.EndSidenote(formats);
                HandleEndToken();
                SkipLongToken();
                return;
            }

            if (longToken == Tokens.headingToken)
            {
                formatter.EndHeading();
                HandleEndToken();
                SkipLongToken();
                return;
            }
            if ((token == '(') || (token == '['))
            {
                citationLevel++;
                AppendToken();
                return;
            }
            if (token == '{')
            {
                citationLevel++;
                SkipToken();
                return;
            }
            if (token == '~')
            {
                citationLevel++;
                formatter.Append('—');
                SkipToken();
                return;
            }
            if ((token == ')') || (token == ']'))
            {
                citationLevel = DecreaseCitationLevel();
                AppendToken();
                return;
            }
            if (token == '}')
            {
                citationLevel = DecreaseCitationLevel();
                SkipToken();
                return;
            }
            if (token == '_')
            {
                formatter.Append(HTMLTags.NonbreakingSpace);
                SkipToken();
                return;
            }
            if (token == '#')
            {
                if (formats.labelExists)
                {
                    SearchForEndBracketToken();
                    if (!mainRange.Valid) return;
                    ResetCitationLevel();
                    formatter.AppendTag(currentParagraph, labelRange, mainRange, formats);
                    formats.labelExists = false;
                    labelRange.Invalidate();
                    mainRange.GoToNextStartPosition();
                    return;
                }
                MoveIndexToEndOfWord();
                formatter.AppendTag(currentParagraph, mainRange, mainRange, formats);
                return;
            }
            AppendToken();
        }

        internal void SetLabel()
        {
            labelRange.Copy(mainRange);
            labelRange.PullEnd();
            mainRange.GoToNextStartPosition();
            formats.labelExists = true;
        }
        private void AppendToken()
        {
            formatter.AppendString(currentParagraph, mainRange.End, mainRange.End);
            mainRange.GoToNextStartPosition();
        }

        private void SkipToken()
        {
            mainRange.GoToNextStartPosition();
        }

        private void SkipLongToken()
        {
            mainRange.BumpEnd();
            mainRange.GoToNextStartPosition();
        }

        private void AppendTextUpToToken()
        {
            formatter.AppendString(currentParagraph, mainRange.Start, mainRange.End - 1);
        }

        protected void HandleEndToken()
        {
            foundEndToken = true;
            //todo handle closing for special tokens?
            formatter.EndSection();
        }

        public override void HandleCitations()
        {
            List<Citation> citations =
                citationHandler.ParseCitations(mainRange, currentParagraph);
            if (citations.Count > 0)
            {
                if (formats.labelExists)
                {
                    citations[Ordinals.first].DisplayLabel = labelRange;
                    formatter.AppendCitationWithLabel(currentParagraph, citations[Ordinals.first]);
                    mainRange.MoveStartTo(mainRange.End);
                    return;
                }
                else
                {
                    formatter.AppendCitations(currentParagraph, citations);
                    mainRange.MoveStartTo(citations[citations.Count - 1].Label.End + 1);
                }
            }
        }

        protected void MoveIndexToEndOfWord()
        {
            mainRange.BumpEnd();
            while ((!mainRange.AtLimit) &&
                (Symbols.IsPartOfWord(currentParagraph.CharAt(mainRange.End))))
                mainRange.BumpEnd();
            mainRange.PullEnd();
        }
    }

}
