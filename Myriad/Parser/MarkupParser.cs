using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Myriad.Library;

namespace Myriad.Parser
{
    public class MarkupParser : BasicMarkupParser
    {
        bool detail;
        bool bold;
        bool super;
        bool italic;
        bool heading;
        readonly PageFormatter formatter;

        public MarkupParser()
        {
            formatter = new PageFormatter(this);
        }

        public StringBuilder ParsedText { get { return formatter.Result; } }

        protected override void HandleEnd()
        {
            mainRange.MoveEndToLimit();
            formatter.AppendString(citationLevel);
        }

        public override void SearchForToken()
        {
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(Tokens.tokens, mainRange.Start));
        }
        override public void HandleToken()
        {
            char token = currentParagraph.CharAt(mainRange.End);
            char charAfterToken = currentParagraph.CharAt(mainRange.End + 1);
            if ((token == '+') && (charAfterToken == '+'))
            {
                detail = formatter.HandleDetails(detail, citationLevel);
                mainRange.BumpStart();
                return;
            }

            if ((token == '*') && (charAfterToken == '*'))
            {
                bold = formatter.ToggleBold(bold, citationLevel);
                mainRange.BumpStart();
                return;
            }
            if (token == '^')
            {
                super = formatter.ToggleSuperscription(super, citationLevel);
                return;
            }
            if (token == '/')
            {
                italic = formatter.ToggleItalic(italic, citationLevel, charAfterToken);
                mainRange.BumpStart();
                return;
            }
            if ((token == ')') && (charAfterToken == ')'))
            {
                formatter.EndSidenote(citationLevel);
                mainRange.BumpStart();
                mainRange.BumpStart();
                return;
            }
            if ((token == '(') && (charAfterToken == '('))
            {
                heading = formatter.StartSidenote(heading);
                mainRange.BumpStart();
                mainRange.BumpStart();
                return;
            }
            if ((token == '[') && (charAfterToken == '['))
            {
                formatter.AppendFigure(currentParagraph.Text);
                return;
            }
            if (token == '=')
            {
                formatter.AppendString();
                heading = formatter.ToggleHeading(heading, charAfterToken);
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
            if (token == '_')
            {
                formatter.AppendString();
                return;
            }
            if (token == '|')
            {
                formatter.SetLabel(citationLevel);
                return;
            }
            if (token == '#')
            {
                if (formatter.LabelExists)
                {
                    MoveIndexToEndBracket();
                    if (!mainRange.Valid) return;
                    ResetCitationLevel();
                    formatter.AppendTag();
                    return;
                }
                formatter.AppendString(citationLevel);

                MoveIndexToEndOfWord();
            }
            formatter.AppendTag();

            formatter.AppendString();
        }
        new void HandleCitations()
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
