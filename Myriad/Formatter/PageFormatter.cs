using System;
using System.Collections.Generic;
using Myriad.Paragraph;
using Myriad.Pages;
using Myriad.Library;
using Myriad.Parser.Helpers;

namespace Myriad.Parser
{
    public class PageFormatter
    {
        readonly CitationRange extendedTarget;

        readonly HTMLWriter writer;

        public string Result { get { return writer.Response(); } }

        public PageFormatter(HTMLWriter writer)
        {
            this.writer = writer;
        }

        internal void StartSection()
        {
            writer.Append(HTMLTags.StartSection);
        }

        internal bool ToggleBold(bool bold)
        {
            if (bold)
            {
                writer.Append(HTMLTags.EndBold);
                bold = false;
            }
            else
            {
                writer.Append(HTMLTags.StartBold);
                bold = true;
            }
            return bold;
        }

        internal void StartParagraph()
        {
            writer.Append(HTMLTags.StartParagraph);
        }

        internal bool ToggleSuperscription(bool super)
        {
            if (super)
            {
                writer.Append(HTMLTags.EndSuper);
                super = false;
            }
            else
            {
                writer.Append(HTMLTags.StartSuper);
                super = true;
            }
            return super;
        }


        internal bool ToggleItalic(bool italic)
        {
            if (italic)
            {
                writer.Append(HTMLTags.EndItalic);
                italic = false;
            }
            else
            {
                writer.Append(HTMLTags.StartItalic);
                italic = true;
            }
            return italic;
        }

        internal void EndParagraph()
        {
            writer.Append(HTMLTags.EndParagraph);
        }

        internal void StartHeading()
        {
            writer.Append(HTMLTags.StartHeader);
        }

        internal void AppendClearDiv()
        {
            writer.StartDivWithClass("clear");
            writer.Append(HTMLTags.EndDiv);
        }

        internal void StartMainHeading()
        {
            writer.Append(HTMLTags.StartMainHeader);
        }

        internal void EndMainHeading()
        {
            writer.Append(HTMLTags.EndMainHeader);
        }

        internal void EndComments()
        {
            writer.Append(HTMLTags.EndDiv);
        }

        internal void StartComments()
        {
            writer.StartDivWithID(HTMLClasses.comments);
        }

        internal void EndHeading()
        {
            writer.Append(HTMLTags.EndHeader);
        }

        internal void Append(char c)
        {
            writer.Append(c);
        }

        internal bool ToggleHeading(Formats formats)
        {
            formats.editable = false;
            if (formats.heading)
            {
                writer.Append(HTMLTags.EndHeader);
                writer.Append(HTMLTags.EndSection);
                return false;
            }
            writer.Append(HTMLTags.StartHeader);
            return true;
        }

        internal void StartEditSpan(ParagraphInfo info)
        {
            if (info.type == ParagraphType.Undefined) return;
            writer.StartSpanWithClass(HTMLClasses.paragraphcontent);
        }

        internal void AppendString(IMarkedUpParagraph paragraph, StringRange range)
        {
            AppendString(paragraph, range.Start, range.End);
        }

        internal void AppendString(IMarkedUpParagraph paragraph, int start, int end)
        {
            if (start > end) return;
            writer.Append(paragraph.SpanAt(start, end));
        }

        internal void EndEditSpan(ParagraphInfo info)
        {
            if (info.type == ParagraphType.Undefined) return;
            writer.Append(HTMLTags.EndSpan);
            writer.Append(HTMLTags.StartSpanWithClass);
            writer.Append(HTMLClasses.editparagraph);
            writer.Append(HTMLTags.CloseQuote);
            writer.Append(HTMLTags.Data_EditType);
            writer.Append((int)info.type);
            writer.Append(HTMLTags.Data_ID);
            writer.Append(info.ID);
            writer.Append(HTMLTags.Data_Index);
            writer.Append(info.index);
            writer.Append(HTMLTags.OnClick);
            writer.Append(JavaScriptFunctions.EditParagraph);
            writer.Append(HTMLTags.EndTag);
            writer.Append(HTMLTags.NonbreakingSpace);
            writer.Append("Edit");
            writer.Append(HTMLTags.EndSpan);
        }

        internal void StartSidenoteWithHeading(Formats formats)
        {
            writer.StartDivWithClass(HTMLClasses.sidenote);
            writer.Append(HTMLTags.StartHeader);
            formats.heading = true;
        }
        internal void StartSidenote()
        {
            writer.StartDivWithClass(HTMLClasses.sidenote);
        }

        internal void EndSection()
        {
            writer.Append(HTMLTags.EndSection);
        }

        internal void EndSidenote(Formats formats)
        {
            writer.Append(HTMLTags.EndParagraph);
            writer.Append(HTMLTags.EndDiv);
            formats.editable = false;
        }

        internal bool HandleDetails(bool detail, Formats formats)
        {
            bool startSpan = false;
            if ((formats.hideDetails) && (!detail))
            {
                writer.Append(HTMLTags.StartSpan);
                startSpan = true;
            }
            if (formats.hideDetails)
            {
                if (detail)
                {
                    detail = false;
                    writer.Append(HTMLTags.EndSpan);
                    writer.Append(HTMLTags.EndSpan);
                }
                else
                {
                    detail = true;
                    if (startSpan)
                    {
                        writer.Append(HTMLTags.EndSpan);
                    }
                    writer.StartSpanWithClass(HTMLClasses.hiddendetail);
                    writer.Append(HTMLTags.StartSpan);
                }
            }
            return detail;
        }


        internal void AppendTag(IMarkedUpParagraph paragraph, StringRange labelRange, StringRange tagRange)
        {
            writer.StartAnchorWithClass(HTMLClasses.link);
            writer.AppendHREF(ArticlePage.pageURL);
            writer.Append(HTMLTags.StartQuery);
            writer.Append(ArticlePage.queryKeyTitle);
            writer.Append(paragraph.
                StringAt(tagRange).Replace(' ', '+').
                Replace('[', '(').Replace(']', ')'));
            AppendExtendedTarget();
            AppendPartialPageLoad(writer);
            writer.Append(HTMLTags.EndTag);
            AppendStringAsLabel(paragraph, labelRange);
            writer.Append(HTMLTags.EndAnchor);
        }

        private static void AppendPartialPageLoad(HTMLWriter writer)
        {
            writer.Append(HTMLTags.Ampersand);
            writer.Append(HTMLClasses.partial);
        }

        internal void Append(string stringToAppend)
        {
            writer.Append(stringToAppend);
        }

        private void AppendStringAsLabel(IMarkedUpParagraph paragraph, StringRange range)
        {
            writer.Append(paragraph.
                StringAt(range.Start, range.End).Replace('_', ' '));
        }

        internal void AppendExtendedTarget()
        {
            if (extendedTarget != null)
            {
                writer.Append(HTMLTags.Ampersand);
                writer.Append(ScripturePage.queryKeyTGStart);
                writer.Append(extendedTarget.StartID);
                writer.Append(HTMLTags.Ampersand);
                writer.Append(ScripturePage.queryKeyTGEnd);
                writer.Append(extendedTarget.EndID);
            }
        }

        public void AppendCitationLabels(IMarkedUpParagraph paragraph, List<Citation> citations)
        {
            foreach (var citation in citations)
            {
                AppendCitationLabel(paragraph, citation);
            }
        }

        internal void AppendCitationLabel(IMarkedUpParagraph paragraph, Citation citation)
        {
            if (citation.LeadingSymbols.Length > 0)
                writer.Append(paragraph.
                    SpanAt(citation.LeadingSymbols.Start, citation.LeadingSymbols.End));
            writer.Append(paragraph.SpanAt(citation.Label.Start,
                citation.Label.End));
            if (citation.TrailingSymbols.Length > 0)
                writer.Append(paragraph.
                    SpanAt(citation.TrailingSymbols.Start, citation.TrailingSymbols.End));
        }

        public void AppendCitations(IMarkedUpParagraph paragraph, List<Citation> citations)
        {
            foreach (var citation in citations)
            {
                AppendCitation(paragraph, citation);
            }
        }
        internal void AppendCitationWithLabel(IMarkedUpParagraph paragraph, Citation citation)
        {
            StartCitationAnchor(writer, citation);
            writer.Append(paragraph.SpanAt(citation.DisplayLabel));
            writer.Append(HTMLTags.EndAnchor);
        }
        internal void AppendCitation(IMarkedUpParagraph paragraph, Citation citation)
        {
            if (citation.LeadingSymbols.Length > 0)
                writer.Append(paragraph.
                    SpanAt(citation.LeadingSymbols.Start, citation.LeadingSymbols.End));
            StartCitationAnchor(writer, citation);
            writer.Append(paragraph.SpanAt(citation.Label.Start,
                citation.Label.End));
            writer.Append(HTMLTags.EndAnchor);
            if (citation.TrailingSymbols.Length > 0)
                writer.Append(paragraph.
                    SpanAt(citation.TrailingSymbols.Start, citation.TrailingSymbols.End));
        }

        public static void StartCitationAnchor(HTMLWriter writer, Citation citation)
        {
            writer.Append(HTMLTags.StartAnchor);
            writer.AppendHREF(PageReferrer.URLs[citation.CitationType]);
            writer.Append(HTMLTags.StartQuery);
            PageReferrer.AppendQuery(writer, citation);
            AppendPartialPageLoad(writer);
            writer.Append(HTMLTags.EndTag);
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
            writer.StartFigure(image.Class);
            writer.StartIMG(image.Path);
            writer.AppendIMGWidth(ImageElement.WidthString);
            writer.AppendClass(image.Class);
            writer.Append(HTMLTags.OnClick);
            writer.Append(JavaScriptFunctions.OpenModalPicture);
            writer.Append(HTMLTags.EndSingleTag);
            writer.Append(HTMLTags.EndFigure);
        }
    }
}