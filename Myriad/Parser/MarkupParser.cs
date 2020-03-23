using System;
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

        public MarkupParser(HTMLResponse builder)
        {
            formatter = new PageFormatter(this, builder);
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
                formatter.StartSidenote(formats);
                mainRange.BumpStart();
                mainRange.BumpStart();
                return false;
            }
            if (token == Tokens.picture)
            {
                formats.editable = false;
                formats.figure = true;
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
            formatter.AppendClearDiv();
        }

        public override void SearchForToken()
        {
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(Tokens.tokens, mainRange.Start));
        }
        override public bool HandleToken()
        {
            char token = currentParagraph.CharAt(mainRange.End);
            int longToken = currentParagraph.TokenAt(mainRange.End);
            if (longToken == Tokens.detail)
            {
                formats.detail = formatter.HandleDetails(formats.detail, citationLevel, formats);
                mainRange.BumpStart();
                return true;
            }

            if (longToken == Tokens.bold)
            {
                formats.bold = formatter.ToggleBold(formats.bold, citationLevel);
                mainRange.BumpStart();
                return true;
            }
            if (token == '^')
            {
                formats.super = formatter.ToggleSuperscription(formats.super, citationLevel);
                return true;
            }
            if (longToken ==Tokens.italic)
            {
                formats.italic = formatter.ToggleItalic(formats.italic, citationLevel);
                mainRange.BumpStart();
                return true;
            }
            if (longToken == Tokens.endSidenote)
            {
                formatter.EndSidenote(citationLevel, formats);
                HandleEndToken();
                return true;
            }

            if (longToken == Tokens.headingToken)
            {
                formatter.AppendString();
                formatter.EndHeading();
                HandleEndToken();
                return true;
            }
            if ((token == '(') || (token == '[') || (token == '{') || (token == '~'))
            {
                formatter.AppendString(citationLevel);
                citationLevel = IncreaseCitationLevel();
                return true;
            }
            if ((token == ')') || (token == ']') || (token == '}'))
            {
                formatter.AppendString(citationLevel);
                citationLevel = DecreaseCitationLevel();
                return true;
            }
            if (token == '_')
            {
                formatter.AppendString();
                return true;
            }
            if (token == '|')
            {
                formatter.SetLabel(citationLevel, formats);
                return true;
            }
            if (token == '#')
            {
                if (formats.labelExists)
                {
                    MoveIndexToEndBracket();
                    if (!mainRange.Valid) return true;
                    ResetCitationLevel();
                    formatter.AppendTag(formats);
                    return true;
                }
                formatter.AppendString(citationLevel);

                MoveIndexToEndOfWord();
                formatter.AppendTag(formats);
                return true;
            }
            return false;
        }

        protected void HandleEndToken()
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
