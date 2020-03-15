using System;
using System.Collections.Generic;
using System.Text;
using Myriad.Library;
using Range = Myriad.Library.Range;


namespace Myriad.Parser

{
    public class MarkupParser
    {
        bool detail;
        bool bold;
        bool super;
        bool italic;
        bool editable;
        bool heading;
        bool sidenote;
        bool figure;
        bool hideDetails = false;
        bool labelExists = false;
        int citationLevel = 0;
        Range extendedTarget;
        MarkedUpParagraph currentParagraph;
        StringRange mainRange;
        readonly StringRange labelRange = new StringRange();
        static readonly char[] tokens = new char[] { '*', '^', '/', '=', '(', '[', '{', ')', ']', '}', '~', '#', '|', '_', '+' };
        static readonly char[] brackettokens = new char[] { '|', '}' }; 
        static readonly Dictionary<char, string> tokenToString = new Dictionary<char, string>
        {
            {'*', "" }, {'/', "" }, {'=', "" }, {'(', "(" }, {'[', "[" }, {'{', "" },
            {')', ")" }, {']', "]" }, {'}', ""}, {'~', "—"}, {'#', "" }, {' ', " " },
            {'_', "&nbsp;" }, {'^',"" }, {'+', ""}
        };
        HTMLStringBuilder builder = new HTMLStringBuilder();
        public StringBuilder ParsedText { get { return builder.Builder; } }

        public void Parse(List<MarkedUpParagraph> paragraphs)
        {
            foreach (MarkedUpParagraph paragraph in paragraphs)
            {
                currentParagraph = paragraph;
                ParseParagraph();
            }
        }

        private void ParseParagraph()
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
        private void HandleToken()
        {
            char token = currentParagraph.CharAt(mainRange.End);
            char charAfterToken = currentParagraph.CharAt(mainRange.End + 1);
            if ((token == '+') && (charAfterToken == '+'))
            {
                detail = HandleDetails(detail, citationLevel);
                mainRange.BumpStart();
                return;
            }

            if ((token == '*') && (charAfterToken == '*'))
            {
                bold = ToggleBold(bold, citationLevel);
                mainRange.BumpStart();
                return;
            }
            if (token == '^')
            {
                super = ToggleSuperscription(super, citationLevel);
                return;
            }
            if (token == '/')
            {
                italic = ToggleItalic(italic, citationLevel, charAfterToken);
                mainRange.BumpStart();
                return;
            }
            if ((token == ')') && (charAfterToken == ')'))
            {
                EndSidenote(citationLevel);
                editable = false;
                mainRange.BumpStart();
                mainRange.BumpStart();
                return;
            }
            if ((token == '(') && (charAfterToken == '('))
            {
                heading = StartSidenote(heading);
                mainRange.BumpStart();
                mainRange.BumpStart();
                return;
            }
            if ((token == '[') && (charAfterToken == '['))
            {
                AppendFigure(currentParagraph.Text);
                figure = true;
                editable = false;
                return;
            }
            if (token == '=')
            {
                AppendString();
                if (charAfterToken == '=')
                {
                    heading = ToggleHeading(heading);
                    if (!heading) return;
                }
                else builder.Append('=');
                return;
            }
            if ((token == '(') || (token == '[') || (token == '{') || (token == '~'))
            {
                citationLevel = IncreaseCitationLevel(citationLevel, token);
                return;
            }
            if ((token == ')') || (token == ']') || (token == '}'))
            {
                citationLevel = DecreaseCitationLevel(citationLevel);
                return;
            }
            if (token == '_')
            {
                AppendString();
                return;
            }
            if (token == '|')
            {
                SetLabel(citationLevel);
                return;
            }
            if (token == '#')
            {
                if (labelExists)
                {
                    labelExists = false;
                    MoveIndexToEndBracket();
                    if (!mainRange.Valid) return;
                    citationLevel = 0;
                    AppendTag();
                    return;
                }
                AppendString(citationLevel);

                MoveIndexToEndOfWord();
            }
            AppendTag();

            AppendString();
        }

        private void MoveIndexToEndBracket()
        {
                if (mainRange.Start > currentParagraph.Length)
                    return;
                mainRange.MoveEndTo(currentParagraph.IndexOf('}', mainRange.Start - 1));
        }

        private void MoveIndexToEndOfWord()
        {
            mainRange.BumpEnd();
            while ((!mainRange.AtLimit) && 
                (Symbols.IsPartOfWord(currentParagraph.CharAt(mainRange.End)))) 
                mainRange.BumpEnd();
            mainRange.PullEnd();
        }

        private bool HandleDetails(bool detail, int citationLevel)
        {
            bool startSpan = false;
            if (((hideDetails) && (!detail)) && (mainRange.Length > 1))
            {
                builder.StartSpan();
                startSpan = true;
            }
            AppendString(citationLevel);
            if (hideDetails)
            {
                if (detail)
                {
                    detail = false;
                    builder.Append("</span></span>");
                }
                else
                {
                    detail = true;
                    if (startSpan)
                    {
                        builder.Append("</span>");
                    }
                    builder.Append("<span class='hiddendetail'><span>");
                }
            }
            return detail;
        }

        private bool ToggleBold(bool bold, int citationLevel)
        {
            AppendString(citationLevel);
            if (bold)
            {
                builder.Append("</b>");
                bold = false;
            }
            else
            {
                builder.Append("<b>");
                bold = true;
            }
            return bold;
        }

        private bool ToggleSuperscription(bool super, int citationLevel)
        {
            AppendString(citationLevel);
            if (super)
            {
                builder.Append("</sup>");
                super = false;
            }
            else
            {
                builder.Append("<sup>");
                super = true;
            }
            return super;
        }

        private bool ToggleItalic(bool italic, int citationLevel, char charAfterToken)
        {
            AppendString(citationLevel);
            if (charAfterToken == '/')
            {
                if (italic)
                {
                    builder.Append("</i>");
                    italic = false;
                }
                else
                {
                    builder.Append("<i>");
                    italic = true;
                }
            }
            else builder.Append('/');
            return italic;
        }

        private bool ToggleHeading(bool heading)
        {
            editable = false;
            if (heading)
            {
                builder.EndHeader();
                mainRange.BumpStart();
                return false;
            }
            builder.StartHeader();

            mainRange.BumpStart();
            return true;
        }

        private bool StartSidenote(bool heading)
        {
            if (currentParagraph.Length > 2)
            {
                builder.StartDivWithClass("sidenote");
                builder.StartHeader();
                heading = true;
            }
            else builder.StartDivWithClass("sidenote");
            sidenote = true;
            editable = false;
            return heading;
        }

        private void EndSidenote(int citationLevel)
        {
            mainRange.PullEnd();
            AppendString(citationLevel);
            editable = false;
            builder.EndParagraph();
            builder.EndDiv();
        }

        internal void AppendTag()
        {
            string tag = currentParagraph.StringAt(mainRange.Start, mainRange.End);
            tag = tag.Replace("#", "");
            tag = tag.Replace("}", "");
            if (labelRange.Valid)
            {
                builder.StartAnchor("link");
                builder.AppendHREF(ArticleModel.pageURL);
                builder.StartQuery();
                builder.Append(ArticleModel.queryKeyTitle);
                mainRange.BumpStart();
                AppendTagString();
                AppendExtendedTarget();
                builder.EndHTMLTag();
                AppendLabel();
                builder.EndAnchor();
                mainRange.MoveStartTo(mainRange.End + 1);
                labelRange.Invalidate();
            }
            else
            {
                builder.StartAnchor("link");
                builder.AppendHREF(ArticleModel.pageURL);
                builder.StartQuery();
                builder.Append(ArticleModel.queryKeyTitle);
                AppendTagStringAnchored();
                AppendExtendedTarget();

                builder.EndHTMLTag();
                AppendStringAsLabel();
                builder.EndAnchor();
            }
        }
        private void AppendStringAsLabel()
        {
            builder.Append(currentParagraph.
                StringAt(mainRange.Start, mainRange.End).Replace('_', ' '));
            mainRange.GoToNextStartPosition();
        }
        private void AppendLabel()
        {
            builder.Append(currentParagraph.StringAt(labelRange.Start, labelRange.End).Replace('_', ' '));
        }
        private void AppendTagString()
        {
            if (mainRange.End < mainRange.Start) return;
            AppendTagStringExclusive(mainRange.End);
            char token = currentParagraph.CharAt(mainRange.End);
            if (tokenToString.ContainsKey(token))
                builder.Append(tokenToString[token]);
            else builder.Append(token);
        }
        private void AppendTagStringAnchored()
        {
            if (mainRange.End <= mainRange.Start) return;
            builder.Append(currentParagraph.
                StringAt(mainRange.Start, mainRange.End).Replace(' ', '+').
                Replace('[', '(').Replace(']', ')'));
        }
        private void AppendTagStringExclusive(int end)
        {
            if (end <= mainRange.Start)
            {
                mainRange.MoveStartTo(end + 1);
                return;
            }
            builder.Append(currentParagraph.StringAt(mainRange.Start, end)
                .Replace('_', '+').Replace('[', '(').Replace(']', ')'));
            mainRange.MoveStartTo(end + 1);
        }
        internal void AppendExtendedTarget()
        {
            if (extendedTarget != null)
            {
                builder.Append("&tgstart=");
                builder.Append(extendedTarget.StartID);
                builder.Append("&tgend=");
                builder.Append(extendedTarget.EndID);
            }
        }
        private void AppendString(int citationLevel)
        {
            if (citationLevel > 0)
            {
                AppendCitations();
            }
            else
            {
                AppendString();
            }
        }
        internal void AppendString()
        {
            if (mainRange.End < mainRange.Start) return;
            AppendStringExclusive(mainRange.End);
            if (mainRange.End < currentParagraph.Length)
            {
                if (tokenToString.ContainsKey(currentParagraph.CharAt(mainRange.End)))
                    builder.Append(tokenToString[currentParagraph.CharAt(mainRange.End)]);
                else builder.Append(currentParagraph.CharAt(mainRange.End));
            }
        }
        private void AppendStringExclusive(int end)
        {
            if (end <= mainRange.Start)
            {
                mainRange.MoveStartTo(end + 1);
                return;
            }
            builder.Append(currentParagraph.StringAt(mainRange.Start, end));
            mainRange.MoveStartTo(end + 1);
        }

        private int IncreaseCitationLevel(int citationLevel, char token)
        {
            AppendString(citationLevel);
            citationLevel++;
            if (token == '{')
            {
                MoveIndexToNextBracketToken();
                mainRange.PullEnd();
            }
            return citationLevel;
        }
        private int DecreaseCitationLevel(int citationLevel)
        {
            AppendString(citationLevel);
            if (citationLevel > 0)
                citationLevel--;
            return citationLevel;
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
        private void SetLabel(int citationLevel)
        {
            if (citationLevel > 0)
            {
                labelRange.Copy(mainRange);
                mainRange.GoToNextStartPosition();
                labelExists = true;
            }
        }
        internal void AppendFigure(string par)
        {
            AppendFigure(new ImageElement(par));
        }

        internal void AppendFigure(ImageElement image)
        {
            if (image == null) return;
            builder.StartFigure(image.Class);
            builder.StartIMG(image.Path);
            builder.AppendIMGWidth(ImageElement.WidthString);
            builder.Append("\" class=");
            builder.Append(image.Class);
            builder.Append(" /></figure>");
        }

    }
}