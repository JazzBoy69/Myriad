using System;
using System.Collections.Generic;
using System.Text;
using Myriad.Pages;
using Myriad.Library;

namespace Myriad.Parser
{
    internal class PageFormatter
    {
        readonly CitationRange extendedTarget;
        readonly StringRange labelRange = new StringRange();
        static readonly Dictionary<char, string> tokenToString = new Dictionary<char, string>
        {
            {'*', "" }, {'/', "" }, {'=', "" }, {'(', "(" }, {'[', "[" }, {'{', "" },
            {')', ")" }, {']', "]" }, {'}', ""}, {'~', "—"}, {'#', "" }, {' ', " " },
            {'_', "&nbsp;" }, {'^',"" }, {'+', ""}
        };

        readonly HTMLResponse builder;
        private readonly IParser parser;
        readonly CitationHandler citationHandler;

        public string Result { get { return builder.Response(); } }

        public PageFormatter(IParser parser, HTMLResponse builder)
        {
            this.parser = parser;
            this.builder = builder;
            citationHandler = new CitationHandler();
        }

        internal void StartSection()
        {
            builder.Append(HTMLTags.StartSection);
        }

        internal bool ToggleBold(bool bold, int citationLevel)
        {
            AppendString(citationLevel);
            if (bold)
            {
                builder.Append(HTMLTags.EndBold);
                bold = false;
            }
            else
            {
                builder.Append(HTMLTags.StartBold);
                bold = true;
            }
            return bold;
        }

        internal void StartParagraph()
        {
            builder.Append(HTMLTags.StartParagraph);
        }

        internal void AppendEndString()
        {
            if (parser.MainRange.End < parser.MainRange.Start) return;
            AppendStringExclusive(parser.MainRange.End);
        }

        internal bool ToggleSuperscription(bool super, int citationLevel)
        {
            AppendString(citationLevel);
            if (super)
            {
                builder.Append(HTMLTags.EndSuper);
                super = false;
            }
            else
            {
                builder.Append(HTMLTags.StartSuper);
                super = true;
            }
            return super;
        }


        internal bool ToggleItalic(bool italic)
        {
            if (italic)
            {
                builder.Append(HTMLTags.EndItalic);
                italic = false;
            }
            else
            {
                builder.Append(HTMLTags.StartItalic);
                italic = true;
            }
            return italic;
        }

        internal void EndParagraph()
        {
            builder.Append(HTMLTags.EndParagraph);
        }

        internal void StartHeading()
        {
            builder.Append(HTMLTags.StartHeader);
        }

        internal void EndHeading()
        {
            builder.Append(HTMLTags.EndHeader);
            builder.Append(HTMLTags.EndSection);
        }
        internal bool ToggleHeading(Formats formats, char charAfterToken)
        {
            formats.editable = false;
            if (formats.heading)
            {
                builder.Append(HTMLTags.EndHeader);
                builder.Append(HTMLTags.EndSection);
                parser.MainRange.BumpStart();
                return false;
            }
            builder.Append(HTMLTags.StartHeader);

            parser.MainRange.BumpStart();
            return true;
        }


        internal void StartSidenote(Formats formats)
        {
            if (parser.CurrentParagraph.Length > 2)
            {
                builder.StartDivWithClass("sidenote");
                builder.Append(HTMLTags.StartHeader);
                formats.heading = true;
            }
            else builder.StartDivWithClass("sidenote");
        }

        internal void EndSection()
        {
            builder.Append(HTMLTags.EndSection);
        }

        internal void EndSidenote(int citationLevel, Formats formats)
        {
            parser.MainRange.PullEnd();
            AppendString(citationLevel);
            builder.Append(HTMLTags.EndParagraph);
            builder.Append(HTMLTags.EndDiv);
            formats.editable = false;
        }

        internal bool HandleDetails(bool detail, int citationLevel, Formats formats)
        {
            bool startSpan = false;
            if (((formats.hideDetails) && (!detail)) && (parser.MainRange.Length > 1))
            {
                builder.Append(HTMLTags.StartSpan);
                startSpan = true;
            }
            AppendString(citationLevel);
            if (formats.hideDetails)
            {
                if (detail)
                {
                    detail = false;
                    builder.Append(HTMLTags.EndSpan);
                    builder.Append(HTMLTags.EndSpan);
                }
                else
                {
                    detail = true;
                    if (startSpan)
                    {
                        builder.Append(HTMLTags.EndSpan);
                    }
                    builder.StartSpan("hiddendetail");
                    builder.Append(HTMLTags.StartSpan);
                }
            }
            return detail;
        }


        internal void AppendTag(Formats formats)
        {
            formats.labelExists = false;
            if (labelRange.Valid)
            {
                builder.StartAnchor("link");
                builder.AppendHREF(ArticlePage.pageURL);
                builder.Append(HTMLTags.StartQuery);
                builder.Append(ArticlePage.queryKey);
                parser.MainRange.BumpStart();
                AppendTagString();
                AppendExtendedTarget();
                builder.Append(HTMLTags.EndTag);
                AppendLabel();
                builder.Append(HTMLTags.EndAnchor);
                parser.MainRange.MoveStartTo(parser.MainRange.End + 1);
                labelRange.Invalidate();
            }
            else
            {
                builder.StartAnchor("link");
                builder.AppendHREF(ArticlePage.pageURL);
                builder.Append(HTMLTags.StartQuery);
                builder.Append(ArticlePage.queryKey);
                AppendTagStringAnchored();
                AppendExtendedTarget();

                builder.Append(HTMLTags.EndTag);
                AppendStringAsLabel();
                builder.Append(HTMLTags.EndAnchor);
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
                builder.Append(HTMLTags.Ampersand);
                builder.Append(ScripturePage.queryKeyTGStart);
                builder.Append(extendedTarget.StartID);
                builder.Append(HTMLTags.Ampersand);
                builder.Append(ScripturePage.queryKeyTGEnd);
                builder.Append(extendedTarget.EndID);
            }
        }
        internal void AppendString(int citationLevel)
        {

            if (citationLevel > 0)
            {
                List<Citation> citations =
                    citationHandler.ParseCitations(parser.MainRange, 
                    parser.CurrentParagraph);
                AppendCitations(citations);
            }
            AppendString();
        }

        private void AppendCitations(List<Citation> citations)
        {
            foreach (var citation in citations)
            {
                AppendCitation(citation);
            }
        }

        private void AppendCitation(Citation citation)
        {
            try
            {
                ScripturePage citationPage = PageReferrer.GetPage(citation.CitationType);
                builder.Append(HTMLTags.StartAnchor);
                builder.AppendHREF(citationPage.GetURL());
                builder.Append(HTMLTags.StartQuery);
                citationPage.AppendQuery(builder, citation);
                builder.Append(HTMLTags.EndTag);
                if (citation.DisplayLabel.Valid)
                    builder.Append(parser.CurrentParagraph.StringAt(citation.DisplayLabel));
                else
                    builder.Append(parser.CurrentParagraph.StringAt(citation.Label));
                builder.Append(HTMLTags.EndAnchor);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        internal void AppendString()
        {
            if (parser.MainRange.End < parser.MainRange.Start) return;
            char token = parser.CurrentParagraph.CharAt(parser.MainRange.End);
            AppendStringExclusive(parser.MainRange.End);
            if (parser.MainRange.End < parser.CurrentParagraph.Length)
            {
                if (tokenToString.ContainsKey(token))
                    builder.Append(tokenToString[token]);
                else builder.Append(token);
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

        internal void AppendFigure(string par, Formats formats)
        {
            AppendFigure(new ImageElement(par));
            formats.figure = true;
            formats.editable = false;
        }

        internal void AppendFigure(ImageElement image)
        {
            if (image == null) return;
            builder.StartFigure(image.Class);
            builder.StartIMG(image.Path);
            builder.AppendIMGWidth(ImageElement.WidthString);
            builder.AppendClass(image.Class);
            builder.Append(HTMLTags.EndSingleTag);
            builder.Append(HTMLTags.EndFigure);
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
        internal void SetLabel(int citationLevel, Formats formats)
        {
            if (citationLevel > 0)
            {
                labelRange.Copy(parser.MainRange);
                parser.MainRange.GoToNextStartPosition();
                formats.labelExists = true;
            }
        }
    }
}