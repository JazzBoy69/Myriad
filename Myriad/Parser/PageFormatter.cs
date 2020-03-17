using Myriad.Library;
using System.Collections.Generic;
using System.Text;

namespace Myriad.Parser
{
    internal class PageFormatter
    {
        bool editable;
        bool figure;
        bool labelExists = false;
        readonly bool hideDetails = false;
        readonly Library.Range extendedTarget;
        readonly StringRange labelRange = new StringRange();
        static readonly Dictionary<char, string> tokenToString = new Dictionary<char, string>
        {
            {'*', "" }, {'/', "" }, {'=', "" }, {'(', "(" }, {'[', "[" }, {'{', "" },
            {')', ")" }, {']', "]" }, {'}', ""}, {'~', "—"}, {'#', "" }, {' ', " " },
            {'_', "&nbsp;" }, {'^',"" }, {'+', ""}
        };
        readonly HTMLStringBuilder builder = new HTMLStringBuilder();
        private readonly IParser parser;
        readonly CitationHandler citationHandler;
        internal bool LabelExists
        {
            get
            {
                return labelExists;
            }
        }

        public StringBuilder Result { get { return builder.Builder; } }

        public PageFormatter(IParser parser)
        {
            this.parser = parser;
            citationHandler = new CitationHandler();
        }

        internal bool ToggleBold(bool bold, int citationLevel)
        {
            AppendString(citationLevel);
            if (bold)
            {
                builder.EndBold();
                bold = false;
            }
            else
            {
                builder.StartBold();
                bold = true;
            }
            return bold;
        }

        internal bool ToggleSuperscription(bool super, int citationLevel)
        {
            AppendString(citationLevel);
            if (super)
            {
                builder.EndSuper();
                super = false;
            }
            else
            {
                builder.StartSuper();
                super = true;
            }
            return super;
        }


        internal bool ToggleItalic(bool italic, int citationLevel, char charAfterToken)
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

        internal bool ToggleHeading(bool heading, char charAfterToken)
        {
            if (charAfterToken == '=')
            {
                editable = false;
                if (heading)
                {
                    builder.EndHeader();
                    parser.MainRange.BumpStart();
                    return false;
                }
                builder.StartHeader();

                parser.MainRange.BumpStart();
                return true;
            }
            else builder.Append('=');
            return heading;
        }


        internal bool StartSidenote(bool heading)
        {
            if (parser.CurrentParagraph.Length > 2)
            {
                builder.StartDivWithClass("sidenote");
                builder.StartHeader();
                heading = true;
            }
            else builder.StartDivWithClass("sidenote");
            editable = false;
            return heading;
        }

        internal void EndSidenote(int citationLevel)
        {
            parser.MainRange.PullEnd();
            AppendString(citationLevel);
            editable = false;
            builder.EndParagraph();
            builder.EndDiv();
            editable = false;
        }

        internal bool HandleDetails(bool detail, int citationLevel)
        {
            bool startSpan = false;
            if (((hideDetails) && (!detail)) && (parser.MainRange.Length > 1))
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
                    builder.EndSpan();
                    builder.EndSpan();
                }
                else
                {
                    detail = true;
                    if (startSpan)
                    {
                        builder.EndSpan();
                    }
                    builder.StartSpan("hiddendetail");
                    builder.StartSpan();
                }
            }
            return detail;
        }


        internal void AppendTag()
        {
            labelExists = false;
            if (labelRange.Valid)
            {
                builder.StartAnchor("link");
                builder.AppendHREF(ArticleModel.pageURL);
                builder.StartQuery();
                builder.Append(ArticleModel.queryKeyTitle);
                parser.MainRange.BumpStart();
                AppendTagString();
                AppendExtendedTarget();
                builder.EndHTMLTag();
                AppendLabel();
                builder.EndAnchor();
                parser.MainRange.MoveStartTo(parser.MainRange.End + 1);
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
            builder.Append(parser.CurrentParagraph.
                StringAt(parser.MainRange.Start, parser.MainRange.End).Replace('_', ' '));
            parser.MainRange.GoToNextStartPosition();
        }
        private void AppendLabel()
        {
            builder.Append(parser.CurrentParagraph.StringAt(labelRange.Start, labelRange.End).Replace('_', ' '));
        }
        private void AppendTagString()
        {
            if (parser.MainRange.End < parser.MainRange.Start) return;
            AppendTagStringExclusive(parser.MainRange.End);
            char token = parser.CurrentParagraph.CharAt(parser.MainRange.End);
            if (tokenToString.ContainsKey(token))
                builder.Append(tokenToString[token]);
            else builder.Append(token);
        }

        private void AppendTagStringAnchored()
        {
            if (parser.MainRange.End <= parser.MainRange.Start) return;
            builder.Append(parser.CurrentParagraph.
                StringAt(parser.MainRange.Start, parser.MainRange.End).Replace(' ', '+').
                Replace('[', '(').Replace(']', ')'));
        }
        private void AppendTagStringExclusive(int end)
        {
            if (end <= parser.MainRange.Start)
            {
                parser.MainRange.MoveStartTo(end + 1);
                return;
            }
            builder.Append(parser.CurrentParagraph.StringAt(parser.MainRange.Start, end)
                .Replace('_', '+').Replace('[', '(').Replace(']', ')'));
            parser.MainRange.MoveStartTo(end + 1);
        }


        internal void AppendExtendedTarget()
        {
            if (extendedTarget != null)
            {
                builder.AppendAmpersand();
                builder.Append(TextModel.queryKeyTGStart);
                builder.Append(extendedTarget.StartID);
                builder.AppendAmpersand();
                builder.Append(TextModel.queryKeyTGEnd);
                builder.Append(extendedTarget.EndID);
            }
        }
        internal void AppendString(int citationLevel)
        {
            if (citationLevel > 0)
            {
                citationHandler.ParseCitations();
            }
            else
            {
                AppendString();
            }
        }

        internal void AppendString()
        {
            if (parser.MainRange.End < parser.MainRange.Start) return;
            AppendStringExclusive(parser.MainRange.End);
            if (parser.MainRange.End < parser.CurrentParagraph.Length)
            {
                if (tokenToString.ContainsKey(parser.CurrentParagraph.CharAt(parser.MainRange.End)))
                    builder.Append(tokenToString[parser.CurrentParagraph.CharAt(parser.MainRange.End)]);
                else builder.Append(parser.CurrentParagraph.CharAt(parser.MainRange.End));
            }
        }
        private void AppendStringExclusive(int end)
        {
            if (end <= parser.MainRange.Start)
            {
                parser.MainRange.MoveStartTo(end + 1);
                return;
            }
            builder.Append(parser.CurrentParagraph.StringAt(parser.MainRange.Start, end));
            parser.MainRange.MoveStartTo(end + 1);
        }

        internal void AppendFigure(string par)
        {
            AppendFigure(new ImageElement(par));
            figure = true;
            editable = false;
        }

        internal void AppendFigure(ImageElement image)
        {
            if (image == null) return;
            builder.StartFigure(image.Class);
            builder.StartIMG(image.Path);
            builder.AppendIMGWidth(ImageElement.WidthString);
            builder.AppendClass(image.Class);
            builder.EndSingleTag();
            builder.EndFigure();
        }
        internal void AppendNextStartCharacter()
        {
            builder.Append(parser.CurrentParagraph.CharAt(parser.MainRange.Start));
            parser.MainRange.BumpStart();
        }
        internal void AppendNextCharacter()
        {
            builder.Append(parser.CurrentParagraph.CharAt(parser.MainRange.Start));
            parser.MainRange.BumpStart();
        }
        internal void SetLabel(int citationLevel)
        {
            if (citationLevel > 0)
            {
                labelRange.Copy(parser.MainRange);
                parser.MainRange.GoToNextStartPosition();
                labelExists = true;
            }
        }
    }
}