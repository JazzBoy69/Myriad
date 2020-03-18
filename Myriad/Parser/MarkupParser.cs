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
        }
        protected override void HandleEnd()
        {
            mainRange.MoveEndToLimit();
            formatter.AppendString(citationLevel);
            formatter.EndSection();
        }

        public override void SearchForToken()
        {
            mainRange.MoveEndTo(currentParagraph.IndexOfAny(Tokens.tokens, mainRange.Start));
        }
        async override public Task HandleToken()
        {
            char token = currentParagraph.CharAt(mainRange.End);
            char charAfterToken = currentParagraph.CharAt(mainRange.End + 1);
            if ((token == '+') && (charAfterToken == '+'))
            {
                formats.detail = formatter.HandleDetails(formats.detail, citationLevel, formats);
                mainRange.BumpStart();
                return;
            }

            if ((token == '*') && (charAfterToken == '*'))
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
            if (token == '/')
            {
                formats.italic = formatter.ToggleItalic(formats.italic, citationLevel, charAfterToken);
                mainRange.BumpStart();
                return;
            }
            if ((token == ')') && (charAfterToken == ')'))
            {
                formatter.EndSidenote(citationLevel, formats);
                mainRange.BumpStart();
                mainRange.BumpStart();
                return;
            }
            if ((token == '(') && (charAfterToken == '('))
            {
                formatter.StartSidenote(formats);
                mainRange.BumpStart();
                mainRange.BumpStart();
                return;
            }
            if ((token == '[') && (charAfterToken == '['))
            {
                formatter.AppendFigure(currentParagraph.Text, formats);
                return;
            }
            if (token == '=')
            {
                formatter.AppendString();
                formats.heading = formatter.ToggleHeading(formats, charAfterToken);
                return;
            }
            if ((token == '(') || (token == '[') || (token == '{') || (token == '~'))
            {
                citationLevel = IncreaseCitationLevel(token);
                return;
            }
            if ((token == ')') || (token == ']') || (token == '}'))
            {
                citationLevel = await DecreaseCitationLevel();
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
            }
            formatter.AppendTag(formats);

            formatter.AppendString();
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
