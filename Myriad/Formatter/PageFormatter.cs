using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Myriad.Pages;
using Myriad.Library;
using Myriad.Parser.Helpers;
using Myriad.Formatter;
using Myriad.Data;

namespace Myriad.Parser
{
    public class PageFormatter
    {
        Citation target;
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


        internal async Task<bool> ToggleItalic(bool italic, bool sidenote)
        {
            if (sidenote)
            {
                if (italic)
                {
                    await writer.Append(HTMLTags.EndSpan);
                    italic = false;
                }
                else
                {
                    await writer.Append(HTMLTags.StartSpanWithClass+
                        HTMLClasses.emphasize+
                        HTMLTags.CloseQuoteEndTag);
                    italic = true;
                }
                return italic;
            }
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

        internal void SetTargetRange(Citation target)
        {
            this.target = target;
        }

        internal async Task StartComments()
        {
            await writer.Append(HTMLTags.StartDivWithClass+
                HTMLClasses.comments+
                HTMLTags.CloseQuoteEndTag);
        }

        internal static async Task WriteTagAnchor(HTMLWriter writer, string label, string tag, Citation target)
        {
            await writer.Append(HTMLTags.StartAnchor +
                HTMLTags.HREF +
                ArticlePage.pageURL +
                HTMLTags.StartQuery +
                ArticlePage.queryKeyTitle +
                Symbol.equal);
            await writer.Append(tag);
            if ((target != null) && target.Valid)
            {
                await writer.Append(HTMLTags.Ampersand +
                    ScripturePage.queryKeyTGStart +
                    Symbol.equal);
                await writer.Append(target.Start);
                await writer.Append(HTMLTags.Ampersand +
                    ScripturePage.queryKeyTGEnd +
                    Symbol.equal);
                await writer.Append(target.EndID.ID);
            }
            await AppendPartialPageLoad(writer);
            await AppendHandleLink(writer);
            await writer.Append(HTMLTags.EndTag);
            await writer.Append(label);
            await writer.Append(HTMLTags.EndAnchor);
        }

        internal static async Task WriteTagAnchor(HTMLWriter writer, string label, int articleID)
        {
            await writer.Append(HTMLTags.StartAnchor +
                HTMLTags.HREF +
                ArticlePage.pageURL +
                HTMLTags.StartQuery +
                ArticlePage.queryKeyID +
                Symbol.equal);
            await writer.Append(articleID);
            await AppendPartialPageLoad(writer);
            await AppendHandleLink(writer);
            await writer.Append(HTMLTags.EndTag);
            await writer.Append(label.Replace('_', ' '));
            await writer.Append(HTMLTags.EndAnchor);
        }

        internal static async Task WriteTagAnchor(HTMLWriter writer, string label, int articleID, Citation target)
        {
            await writer.Append(HTMLTags.StartAnchor +
                HTMLTags.HREF +
                ArticlePage.pageURL +
                HTMLTags.StartQuery +
                ArticlePage.queryKeyID +
                Symbol.equal);
            await writer.Append(articleID);
            await writer.Append(HTMLTags.Ampersand +
                ScripturePage.queryKeyTGStart +
                Symbol.equal);
            await writer.Append(target.Start);
            await writer.Append(HTMLTags.Ampersand +
                ScripturePage.queryKeyTGEnd +
                Symbol.equal);
            await writer.Append(target.EndID.ID);
            await AppendPartialPageLoad(writer);
            await AppendHandleLink(writer);
            await writer.Append(HTMLTags.EndTag);
            await writer.Append(label.Replace('_', ' '));
            await writer.Append(HTMLTags.EndAnchor);
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

        internal async Task<bool> HandleDetails(Formats formats)
        {
            bool startSpan = false;
            bool detail = formats.detail;
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
            await AppendTargetRange();
            await AppendPartialPageLoad(writer);
            await AppendHandleLink(writer);
            await writer.Append(HTMLTags.EndTag);
            await AppendStringAsLabel(paragraph, labelRange);
            await writer.Append(HTMLTags.EndAnchor);
        }

        internal static async Task AppendPartialPageLoad(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.Ampersand +
                HTMLClasses.partial);
        }

        internal static async Task AppendHandleLink(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.OnClick +
                JavaScriptFunctions.HandleLink);
        }

        internal static async Task AppendHandleParagraphClick(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.OnClick +
                JavaScriptFunctions.HandleParagraphTextClick);
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

        internal async Task AppendTargetRange()
        {
            if (target != null)
            {
                await writer.Append(HTMLTags.Ampersand +
                ScripturePage.queryKeyTGStart+Symbol.equal);
                await writer.Append(target.Start);
                await writer.Append(HTMLTags.Ampersand);
                await writer.Append(ScripturePage.queryKeyTGEnd+Symbol.equal);
                await writer.Append(target.End);
            }
        }

        public async Task AppendCitationLabels(IParagraph paragraph, List<Citation> citations)
        {
            for (int i=Ordinals.first; i<citations.Count; i++)
            {
                await AppendCitationLabel(paragraph, citations[i]);
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
            if ((target != null) && target.Valid)
            {
                await CitationConverter.AppendLinks(writer, citations, target);
            }
            else
            {
                await CitationConverter.AppendLinks(writer, citations);
            }
        }

        private async Task AppendText(IParagraph paragraph, Citation citation)
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

        internal async Task AppendCitationWithLabel(IParagraph paragraph, Citation citation)
        {
            await StartCitationAnchor(writer, citation);

            await writer.Append(paragraph.SpanAt(citation.DisplayLabel).ToString());
            await writer.Append(HTMLTags.EndAnchor);
        }

        internal async Task StartCitationAnchor(HTMLWriter writer, Citation citation)
        {
            await writer.Append(HTMLTags.StartAnchor);
            if ((target != null) &&
                (target.Contains(citation.Key) ||
                citation.Contains(target.Key)))
            {
                await writer.Append(HTMLTags.Class +
                    HTMLClasses.target +
                    HTMLTags.CloseQuote +
                    Symbol.space);
            }
            await writer.Append(HTMLTags.HREF);
            await writer.Append(SidebarPageReferrer.URLs[citation.CitationType]);
            await writer.Append(HTMLTags.StartQuery);
            await AppendQuery(writer, citation);
            await AppendPartialPageLoad(writer);
            await AppendHandleLink(writer);
            await writer.Append(HTMLTags.EndTag);
        }

        internal static async Task StartCitationAnchor(HTMLWriter writer, Citation citation, CitationRange target)
        {
            await writer.Append(HTMLTags.StartAnchor);
            if ((target != null) &&
                (target.Contains(citation.Key) ||
                citation.Contains(target.Key)))
            {
                await writer.Append(HTMLTags.Class +
                    HTMLClasses.target +
                    HTMLTags.CloseQuote +
                    Symbol.space);
            }
            await writer.Append(HTMLTags.HREF);
            await writer.Append(SidebarPageReferrer.URLs[citation.CitationType]);
            await writer.Append(HTMLTags.StartQuery);
            await AppendQuery(writer, citation);
            await AppendPartialPageLoad(writer);
            await AppendHandleLink(writer);
            await writer.Append(HTMLTags.EndTag);
        }


        public static async Task StartCitationLink(HTMLWriter writer, Citation citation)
        {
            await writer.Append(HTMLTags.StartAnchor);
            await writer.Append(HTMLTags.HREF);
            await writer.Append(PageReferrer.URLs[citation.CitationType]);
            await writer.Append(HTMLTags.StartQuery);
            await AppendQuery(writer, citation);
            await AppendPartialPageLoad(writer);
            await AppendHandleLink(writer);
            await writer.Append(HTMLTags.EndTag);
        }
        public static async Task StartSpanCitationLink(HTMLWriter writer, Citation citation, bool multi)
        {
            await writer.Append(HTMLTags.StartAnchor);
            await writer.Append(HTMLTags.HREF);
            await writer.Append(PageReferrer.URLs[citation.CitationType]);
            await writer.Append(HTMLTags.StartQuery);
            await AppendQuery(writer, citation);
            await AppendPartialPageLoad(writer);
            if (multi)
            {
                await AppendHandleParagraphClick(writer);
            }
            else
            {
                await AppendHandleLink(writer);
            }
            await writer.Append(HTMLTags.EndTag);
        }

        public static async Task StartSidebarCitationLink(HTMLWriter writer, Citation citation)
        {
            await writer.Append(HTMLTags.StartAnchor);
            await writer.Append(HTMLTags.HREF);
            await writer.Append(SidebarPageReferrer.URLs[citation.CitationType]);
            await writer.Append(HTMLTags.StartQuery);
            await AppendQuery(writer, citation);
            await AppendPartialPageLoad(writer);
            await AppendHandleLink(writer);
            await writer.Append(HTMLTags.EndTag);
        }

        public static async Task StartCitationLink(HTMLWriter writer, Citation citation, Citation targetCitation)
        {
            await writer.Append(HTMLTags.StartAnchor);
            await writer.Append(HTMLTags.HREF);
            await writer.Append(SidebarPageReferrer.URLs[citation.CitationType]);
            await writer.Append(HTMLTags.StartQuery);
            await AppendQuery(writer, citation, targetCitation);
            await AppendPartialPageLoad(writer);
            await AppendHandleLink(writer);
            await writer.Append(HTMLTags.EndTag);
        }

        internal static async Task AppendQuery(HTMLWriter writer, Citation citation)
        {
            await writer.Append("start=");
            await writer.Append(citation.Start);
            await writer.Append("&end=");
            await writer.Append(citation.End);
            if (citation.WordIndexIsDeferred)
            {
                await writer.Append("&word=");
                await writer.Append(citation.Word);
            }
            if ((citation.CitationType == CitationTypes.Chapter) || citation.Navigating)
            {
                await writer.Append("&navigating=true");
            }
        }
        internal static async Task AppendQuery(HTMLWriter writer, Citation citation, Citation targetCitation)
        {
            await writer.Append(ScripturePage.queryKeyStart+Symbol.equal);
            await writer.Append(citation.Start);
            await writer.Append(HTMLTags.Ampersand+ScripturePage.queryKeyEnd+Symbol.equal);
            await writer.Append(citation.End);
            if (citation.WordIndexIsDeferred)
            {
                await writer.Append(HTMLTags.Ampersand+ScripturePage.queryKeyWord+Symbol.equal);
                await writer.Append(citation.Word);
            }
            if (targetCitation != null)
            {
                await writer.Append(HTMLTags.Ampersand + ScripturePage.queryKeyTGStart + Symbol.equal);
                await writer.Append(targetCitation.Start);
                await writer.Append(HTMLTags.Ampersand + ScripturePage.queryKeyTGEnd + Symbol.equal);
                await writer.Append(targetCitation.End);
            }
            if ((citation.CitationType == CitationTypes.Chapter) || citation.Navigating)
            {
                await writer.Append("&navigating=true");
            }
        }
        internal async Task AppendFigure(string par, Formats formats)
        {
            int index = par.IndexOf("]]");
            string filename = par.Substring(Ordinals.third, index - 2);
            await AppendFigure(new ImageElement(filename));
            formats.figure = true;
            formats.editable = false;
        }

        internal async Task AppendFigure(ImageElement image)
        {
            if (image == null) return;
            await writer.Append(HTMLTags.StartFigureWithClass);
            await writer.Append(image.Class);
            await writer.Append(HTMLTags.CloseQuoteEndTag);
            await writer.Append(image.ToString());
            await writer.Append(HTMLTags.EndFigure);
        }
    }
}