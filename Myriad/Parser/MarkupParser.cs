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

    }
    public class MarkupParser : BasicMarkupParser
    {
        readonly Formats formats = new Formats();
        readonly PageFormatter formatter;

        public MarkupParser(HTMLResponse builder)
        {
            formatter = new PageFormatter(this, builder);
        }

        public string ParsedText { get { return formatter.Result; } }

        protected override void HandleStart()
        {
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

        private bool HandleStartToken()
        {
            int token = currentParagraph.TokenAt(Ordinals.first);
            if (token == Tokens.headingToken)
            {
                formats.editable = false;
                formatter.StartHeading();
                return false;
            }

            if (token == Tokens.startSidenote)
            {
                formats.editable = false;
                formatter.StartSidenote(formats);
                mainRange.BumpStart();
                mainRange.BumpStart();
                return false;
            }
            if (token == Tokens.picture)
            {
                formatter.AppendFigure(currentParagraph.Text, formats);
                return true;
            }
            //Todo Table

            return false;
        }

        protected override void HandleEnd()
        {
            mainRange.MoveEndToLimit();
            formatter.AppendEndString();
            formatter.EndParagraph();
            formatter.EndSection();
        }

        public override void SearchForToken()
        {
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(Tokens.tokens, mainRange.Start));
        }
        override public void HandleToken()
        {
            char token = currentParagraph.CharAt(mainRange.End);
            int longToken = currentParagraph.TokenAt(mainRange.End);
            if (longToken == Tokens.detail)
            {
                formats.detail = formatter.HandleDetails(formats.detail, citationLevel, formats);
                mainRange.BumpStart();
                return;
            }

            if (longToken == Tokens.bold)
            {
                formats.bold = formatter.ToggleBold(formats.bold, citationLevel);
                mainRange.BumpStart();
                return;
            }
            if (token == '^')
            {
                formats.super = formatter.ToggleSuperscription(formats.super, citationLevel);
                return;
            }
            if (longToken ==Tokens.italic)
            {
                formats.italic = formatter.ToggleItalic(formats.italic);
                mainRange.BumpStart();
                return;
            }
            if (longToken == Tokens.endSidenote)
            {
                formatter.EndSidenote(citationLevel, formats);
                HandleEndToken();
                return;
            }

            if (longToken == Tokens.headingToken)
            {
                formatter.AppendString();
                formatter.EndHeading();
                HandleEndToken();
                return;
            }
            if ((token == '(') || (token == '[') || (token == '{') || (token == '~'))
            {
                citationLevel = IncreaseCitationLevel();
                return;
            }
            if ((token == ')') || (token == ']') || (token == '}'))
            {
                citationLevel = DecreaseCitationLevel();
                return;
            }
            if (token == '_')
            {
                formatter.AppendString();
                return;
            }
            if (token == '|')
            {
                formatter.SetLabel(citationLevel, formats);
                return;
            }
            if (token == '#')
            {
                if (formats.labelExists)
                {
                    MoveIndexToEndBracket();
                    if (!mainRange.Valid) return;
                    ResetCitationLevel();
                    formatter.AppendTag(formats);
                    return;
                }
                formatter.AppendString(citationLevel);

                MoveIndexToEndOfWord();
                formatter.AppendTag(formats);
            }
            formatter.AppendString();
        }

        private void HandleEndToken()
        {
            foundEndToken = true;
            //TODO handle closing for special tokens
            formatter.EndSection();
        }

        public override void HandleCitations()
        {
            formatter.AppendString(citationLevel);
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
