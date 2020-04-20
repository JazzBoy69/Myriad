using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
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

        internal async Task<bool> ToggleBold(bool bold)
        {
            if (bold)
            {
                await writer.Append(HTMLTags.EndBold);
                bold = false;
            }
            else
            {
                await writer.Append(HTMLTags.StartBold);
                bold = true;
            }
            return bold;
        }

        internal async Task<bool> ToggleSuperscription(bool super)
        {
            if (super)
            {
                await writer.Append(HTMLTags.EndSuper);
                super = false;
            }
            else
            {
                await writer.Append(HTMLTags.StartSuper);
                super = true;
            }
            return super;
        }


        internal async Task<bool> ToggleItalic(bool italic)
        {
            if (italic)
            {
                await writer.Append(HTMLTags.EndItalic);
                italic = false;
            }
            else
            {
                await writer.Append(HTMLTags.StartItalic);
                italic = true;
            }
            return italic;
        }

        internal async Task AppendClearDiv()
        {
            await writer.Append(HTMLTags.StartDivWithClass+
                HTMLClasses.clear+
                HTMLTags.CloseQuoteEndTag
                +HTMLTags.EndDiv);
        }

        internal async Task StartComments()
        {
            await writer.Append(HTMLTags.StartDivWithClass+
                HTMLClasses.comments+
                HTMLTags.CloseQuoteEndTag);
        }


        internal async Task<bool> ToggleHeading(Formats formats)
        {
            formats.editable = false;
            if (formats.heading)
            {
                await writer.Append(HTMLTags.EndHeader+
                    HTMLTags.EndSection);
                return false;
            }
            await writer.Append(HTMLTags.StartHeader);
            return true;
        }

        internal async Task StartEditSpan(ParagraphInfo info)
        {
            if (info.type == ParagraphType.Undefined) return;
            await writer.Append(HTMLTags.StartSpanWithClass+
                HTMLClasses.paragraphcontent+
                HTMLTags.CloseQuoteEndTag);
        }

        internal async Task AppendString(IParagraph paragraph, StringRange range)
        {
            await AppendString(paragraph, range.Start, range.End);
        }

        internal async Task AppendString(IParagraph paragraph, int start, int end)
        {
            if (start > end) return;
            await writer.Append(paragraph.SpanAt(start, end).ToString());
        }

        internal async Task EndEditSpan(ParagraphInfo info)
        {
            if (info.type == ParagraphType.Undefined) return;
            await writer.Append(HTMLTags.EndSpan+
                HTMLTags.StartSpanWithClass +
                HTMLClasses.editparagraph +
                HTMLTags.CloseQuote +
                HTMLClasses.Data_EditType);
            await writer.Append((int)info.type);
            await writer.Append(HTMLClasses.Data_ID);
            await writer.Append(info.ID);
            await writer.Append(HTMLClasses.Data_Index);
            await writer.Append(info.index);
            await writer.Append(HTMLTags.OnClick +
                JavaScriptFunctions.EditParagraph +
                HTMLTags.EndTag +
                HTMLTags.NonbreakingSpace);
            await writer.Append("Edit");
            await writer.Append(HTMLTags.EndAnchor +
                HTMLTags.EndSpan);
        }

        internal async Task StartSidenoteWithHeading(Formats formats)
        {
            await StartSidenote();
            await writer.Append(HTMLTags.StartHeader);
            formats.heading = true;
        }
        internal async Task StartSidenote()
        {
            await writer.Append(HTMLTags.StartDivWithClass +
                HTMLClasses.sidenote +
                HTMLTags.CloseQuoteEndTag);
        }

        internal async Task EndSidenote(Formats formats)
        {
            await writer.Append(HTMLTags.EndParagraph +
                HTMLTags.EndDiv);
            formats.editable = false;
        }

        internal async Task<bool> HandleDetails(bool detail, Formats formats)
        {
            bool startSpan = false;
            if ((formats.hideDetails) && (!detail))
            {
                await writer.Append(HTMLTags.StartSpan);
                startSpan = true;
            }
            if (formats.hideDetails)
            {
                if (detail)
                {
                    detail = false;
                    await writer.Append(HTMLTags.EndSpan +
                        HTMLTags.EndSpan);
                }
                else
                {
                    detail = true;
                    if (startSpan)
                    {
                        await writer.Append(HTMLTags.EndSpan);
                    }
                    await writer.Append(HTMLTags.StartSpanWithClass +
                        HTMLClasses.hiddendetail +
                        HTMLTags.CloseQuoteEndTag +
                        HTMLTags.StartSpan);
                }
            }
            return detail;
        }


        internal async Task AppendTag(IParagraph paragraph, StringRange labelRange, StringRange tagRange)
        {
            await writer.Append(HTMLTags.StartAnchorWithClass +
                HTMLClasses.link +
                HTMLTags.CloseQuote +
                HTMLTags.HREF +
                ArticlePage.pageURL +
                HTMLTags.StartQuery +
                ArticlePage.queryKeyTitle+'=');
            await writer.Append(paragraph.
                StringAt(tagRange).Replace(' ', '+').
                Replace('[', '(').Replace(']', ')'));
            await AppendExtendedTarget();
            await AppendPartialPageLoad(writer);
            await AppendHandleLink(writer);
            await writer.Append(HTMLTags.EndTag);
            await AppendStringAsLabel(paragraph, labelRange);
            await writer.Append(HTMLTags.EndAnchor);
        }

        private static async Task AppendPartialPageLoad(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.Ampersand +
                HTMLClasses.partial);
        }

        private static async Task AppendHandleLink(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.OnClick +
                JavaScriptFunctions.HandleLink);
        }

        internal async Task Append(string stringToAppend)
        {
            await writer.Append(stringToAppend);
        }

        private async Task AppendStringAsLabel(IParagraph paragraph, StringRange range)
        {
            await writer.Append(paragraph.
                StringAt(range.Start, range.End).Replace('_', ' '));
        }

        internal async Task AppendExtendedTarget()
        {
            if (extendedTarget != null)
            {
                await writer.Append(HTMLTags.Ampersand +
                ScripturePage.queryKeyTGStart);
                await writer.Append(extendedTarget.StartID.ID);
                await writer.Append(HTMLTags.Ampersand);
                await writer.Append(ScripturePage.queryKeyTGEnd);
                await writer.Append(extendedTarget.EndID.ID);
            }
        }

        public async Task AppendCitationLabels(IParagraph paragraph, List<Citation> citations)
        {
            foreach (var citation in citations)
            {
                await AppendCitationLabel(paragraph, citation);
            }
        }

        internal async Task AppendCitationLabel(IParagraph paragraph, Citation citation)
        {
            if (citation.LeadingSymbols.Length > 0)
                await writer.Append(paragraph.
                    SpanAt(citation.LeadingSymbols.Start, citation.LeadingSymbols.End).ToString());
            await writer.Append(paragraph.SpanAt(citation.Label.Start,
                citation.Label.End).ToString());
            if (citation.TrailingSymbols.Length > 0)
                await writer.Append(paragraph.
                    SpanAt(citation.TrailingSymbols.Start, citation.TrailingSymbols.End).ToString());
        }

        public async Task AppendCitations(IParagraph paragraph, List<Citation> citations)
        {
            foreach (var citation in citations)
            {
                await AppendCitation(paragraph, citation);
            }
        }
        internal async Task AppendCitationWithLabel(IParagraph paragraph, Citation citation)
        {
            await StartCitationAnchor(writer, citation);
            await writer.Append(paragraph.SpanAt(citation.DisplayLabel).ToString());
            await writer.Append(HTMLTags.EndAnchor);
        }
        internal async Task AppendCitation(IParagraph paragraph, Citation citation)
        {
            if (citation.LeadingSymbols.Length > 0)
                await writer.Append(paragraph.
                    SpanAt(citation.LeadingSymbols.Start, citation.LeadingSymbols.End).ToString());
            await StartCitationAnchor(writer, citation);
            await writer.Append(paragraph.SpanAt(citation.Label.Start,
                citation.Label.End).ToString());
            await writer.Append(HTMLTags.EndAnchor);
            if (citation.TrailingSymbols.Length > 0)
                await writer.Append(paragraph.
                    SpanAt(citation.TrailingSymbols.Start, citation.TrailingSymbols.End).ToString());
        }

        public static async Task StartCitationAnchor(HTMLWriter writer, Citation citation)
        {
            await writer.Append(HTMLTags.StartAnchor +
                HTMLTags.HREF);
            await writer.Append(PageReferrer.URLs[citation.CitationType]);
            await writer.Append(HTMLTags.StartQuery);
            await AppendQuery(writer, citation);
            await AppendPartialPageLoad(writer);
            await AppendHandleLink(writer);
            await writer.Append(HTMLTags.EndTag);
        }

        internal static async Task AppendQuery(HTMLWriter writer, Citation citation)
        {
            await writer.Append("start=");
            await writer.Append(citation.CitationRange.StartID.ID);
            await writer.Append("&end=");
            await writer.Append(citation.CitationRange.EndID.ID);
            if (citation.CitationRange.WordIndexIsDeferred)
            {
                await writer.Append("&word=");
                await writer.Append(citation.CitationRange.Word);
            }
            if (citation.CitationType == CitationTypes.Chapter)
            {
                await writer.Append("&navigating=true");
            }
        }
        internal async Task AppendFigure(string par, Formats formats)
        {
            string filename = par.Replace("[[", "").Replace("]]", "");
            await AppendFigure(new ImageElement(filename));
            formats.figure = true;
            formats.editable = false;
        }

        internal async Task AppendFigure(ImageElement image)
        {
            if (image == null) return;
            await writer.Append(HTMLTags.StartFigureWithClass);
            await writer.Append(image.Class);
            await writer.Append(HTMLTags.CloseQuoteEndTag+
                HTMLTags.StartImg);
            await writer.Append(image.Path);
            await writer.Append(HTMLTags.CloseQuote+
                HTMLTags.Width);
            await writer.Append("100%");
            await writer.Append(HTMLTags.CloseQuote+
                HTMLTags.Class);
            await writer.Append(image.Class);
            await writer.Append(HTMLTags.CloseQuote+
                HTMLTags.OnClick+
                JavaScriptFunctions.OpenModalPicture+
                HTMLTags.EndSingleTag+
                HTMLTags.EndFigure);
        }
    }
}