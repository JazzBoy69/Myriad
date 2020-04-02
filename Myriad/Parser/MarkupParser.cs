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
        protected readonly PageFormatter formatter;
        readonly CitationHandler citationHandler;
        readonly StringRange labelRange = new StringRange();
        protected ParagraphInfo paragraphInfo;

        public string ParsedText { get { return formatter.Result; } }

        public MarkupParser(HTMLResponse builder)
        {
            formatter = new PageFormatter(this, builder);
            citationHandler = new CitationHandler();
            paragraphInfo.type = ParagraphType.Undefined;
        }

        public void SetParagraphInfo(ParagraphType type, int ID)
        {
            paragraphInfo.type = type;
            paragraphInfo.ID = ID;
        }
        protected override void HandleStart()
        {
            citationLevel = 0;
            formats.Reset();
            AddHTMLBeforeParagraph();
            if (currentParagraph.Length > 1)
            {
                foundEndToken = HandleStartToken();
            }
        }

        virtual protected void AddHTMLBeforeParagraph()
        {

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
            if (citationLevel > 0)
            {
                mainRange.MoveStartTo(lastDash + 1);
                mainRange.PullEnd();
                HandleLastDashCitations();
                mainRange.BumpEnd();
            }
            formatter.AppendString(currentParagraph, mainRange);

            if (formats.heading)
                formatter.EndHeading();
        }

        virtual protected void AddHTMLAfterParagraph()
        {
            if (formats.heading)
                formatter.EndHeading();
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
            if ((mainRange.End == Result.notfound) || ((mainRange.Start < lastDash) && (mainRange.End > lastDash)))
            {
                mainRange.MoveEndTo(lastDash);
            }
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
            if ((token == '(') || (token == '[') || (token=='—'))
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
                StringRange inlineLabelRange = new StringRange(mainRange.End+1, mainRange.End+1);
                MoveIndexToEndOfWord();
                inlineLabelRange.MoveEndTo(mainRange.End-1);
                formatter.AppendTag(currentParagraph, inlineLabelRange, inlineLabelRange, formats);
                mainRange.GoToNextStartPosition();
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

        public void HandleLastDashCitations()
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
                    citations.Last().Label.BumpEnd();
                    formatter.AppendCitations(currentParagraph, citations);
                    mainRange.MoveStartTo(citations[citations.Count - 1].Label.End+1);
                }
            }
        }
        public override void HandleCitations()
        {
            var rangeToParse = new StringRange(mainRange.Start, mainRange.End - 1);
            List<Citation> citations =
                citationHandler.ParseCitations(rangeToParse, currentParagraph);
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
        }
    }

}
