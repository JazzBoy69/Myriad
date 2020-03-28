﻿using System;
using System.Collections.Generic;
using System.Text;
using Myriad.Pages;
using Myriad.Library;

namespace Myriad.Parser
{
    public class PageFormatter
    {
        readonly CitationRange extendedTarget;

        readonly HTMLResponse builder;
        private readonly MarkupParser parser;

        public string Result { get { return builder.Response(); } }

        public PageFormatter(IParser parser, HTMLResponse builder)
        {
            this.parser = (MarkupParser) parser;
            this.builder = builder;
        }

        internal void StartSection()
        {
            builder.Append(HTMLTags.StartSection);
        }

        internal bool ToggleBold(bool bold)
        {
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
            builder.Append(parser.CurrentParagraph.SpanAt(parser.MainRange.Start,
                parser.MainRange.End));
          }

        internal bool ToggleSuperscription(bool super)
        {
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

        internal void AppendClearDiv()
        {
            builder.StartDivWithClass("clear");
            builder.Append(HTMLTags.EndDiv);
        }

        internal void StartMainHeading()
        {
            builder.Append(HTMLTags.StartMainHeader);
        }

        internal void EndMainHeading()
        {
            builder.Append(HTMLTags.EndMainHeader);
        }

        internal void EndComments()
        {
            builder.Append(HTMLTags.EndDiv);
        }

        internal void StartComments()
        {
            builder.StartDivWithID(HTMLClasses.comments);
        }

        internal void EndHeading()
        {
            builder.Append(HTMLTags.EndHeader);
        }

        internal void Append(char c)
        {
            builder.Append(c);
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

        internal void AppendString(IMarkedUpParagraph paragraph, int start, int end)
        {
            if (start > end) return;
            builder.Append(paragraph.SpanAt(start, end));
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

        internal void EndSidenote(Formats formats)
        {
            builder.Append(HTMLTags.EndParagraph);
            builder.Append(HTMLTags.EndDiv);
            formats.editable = false;
        }

        internal bool HandleDetails(bool detail, Formats formats)
        {
            bool startSpan = false;
            if (((formats.hideDetails) && (!detail)) && (parser.MainRange.Length > 1))
            {
                builder.Append(HTMLTags.StartSpan);
                startSpan = true;
            }
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
                    builder.StartSpanWithClass(HTMLClasses.hiddendetail);
                    builder.Append(HTMLTags.StartSpan);
                }
            }
            return detail;
        }


        internal void AppendTag(IMarkedUpParagraph paragraph, StringRange labelRange, Formats formats)
        {
            builder.StartAnchorWithClass(HTMLClasses.link);
            builder.AppendHREF(ArticlePage.pageURL);
            builder.Append(HTMLTags.StartQuery);
            builder.Append(ArticlePage.queryKeyTitle);
            AppendTagStringAnchored();
            AppendExtendedTarget();

            builder.Append(HTMLTags.EndTag);
            AppendStringAsLabel(paragraph, labelRange);
            parser.MainRange.GoToNextStartPosition();
            builder.Append(HTMLTags.EndAnchor);
        }

        internal void Append(string v)
        {
            throw new NotImplementedException();
        }

        private void AppendStringAsLabel(IMarkedUpParagraph paragraph, StringRange range)
        {
            builder.Append(paragraph.
                StringAt(range.Start, range.End).Replace('_', ' '));
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

        internal void AppendCitations(List<Citation> citations)
        {
            foreach (var citation in citations)
            {
                AppendCitation(citation);
            }
        }
        internal void AppendCitationWithLabel(Citation citation)
        {
            StartCitationAnchor(builder, citation);
            builder.Append(parser.CurrentParagraph.SpanAt(citation.DisplayLabel));
            builder.Append(HTMLTags.EndAnchor);
        }
        internal void AppendCitation(Citation citation)
        {
            if (citation.LeadingSymbols.Length > 0)
                builder.Append(parser.CurrentParagraph.
                    SpanAt(citation.LeadingSymbols.Start, citation.LeadingSymbols.End));
            StartCitationAnchor(builder, citation);
            builder.Append(parser.CurrentParagraph.SpanAt(citation.Label.Start,
                citation.Label.End - 1));
            builder.Append(HTMLTags.EndAnchor);
            if (citation.TrailingSymbols.Length > 0)
                builder.Append(parser.CurrentParagraph.
                    SpanAt(citation.TrailingSymbols.Start, citation.TrailingSymbols.End));
        }

        public static void StartCitationAnchor(HTMLResponse builder, Citation citation)
        {
            builder.Append(HTMLTags.StartAnchor);
            builder.AppendHREF(PageReferrer.URLs[citation.CitationType]);
            builder.Append(HTMLTags.StartQuery);
            PageReferrer.AppendQuery(builder, citation);
            builder.Append(HTMLTags.EndTag);
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
    }
}