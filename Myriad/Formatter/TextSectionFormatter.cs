using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Feliciana.Data;
using Myriad.Parser;
using Myriad.Library;
using Myriad.Data;

namespace Myriad.Formatter
{
    public class TextSectionFormatter
    {
        List<string> paragraphs;
        readonly HTMLWriter writer;
        TextFormatter formatter;
        readonly FigureFormatter figureFormatter;
        int headerCount = Ordinals.first;

        internal PageParser Parser { get; }
        public TextSectionFormatter(HTMLWriter writer)
        {
            this.writer = writer;
            Parser = new PageParser(writer);
            figureFormatter = new FigureFormatter(writer);
        }

        public async Task AddTextSection(TextSections textSections, int sectionIndex)
        {
            List<(int start, int end)> idRanges = await ReadLinks(textSections.CommentIDs[sectionIndex]);

            Parser.SetParagraphInfo(ParagraphType.Comment, textSections.CommentIDs[sectionIndex]);
            paragraphs = ReadParagraphs(textSections.CommentIDs[sectionIndex]);
            if (idRanges.Count > 1)
            {
                await AppendTextHeader(textSections);
                await writer.Append(HTMLTags.EndHeader);
                await AppendFigures(textSections);
                await AddTextTabs(idRanges, sectionIndex, textSections);
                await writer.Append(HTMLTags.EndDiv);
                await AddScriptureTextToTabs(idRanges, sectionIndex, textSections);
            }
            else
            {
                await AppendTextHeader(textSections);
                await AppendCitationReference(idRanges[Ordinals.first]);
                await writer.Append(HTMLTags.EndHeader);
                await AppendFigures(textSections); 
                await writer.Append(HTMLTags.EndDiv);
                await AddScriptureText(idRanges[Ordinals.first], textSections);
            }
            await AddComment(textSections);
        }

        public async Task StartTextSection(int commentID, TextSections textSections)
        {
            List<(int start, int end)> idRanges = await ReadLinks(commentID);

            Parser.SetParagraphInfo(ParagraphType.Comment, commentID);
            if (idRanges.Count > 1)
            {
                await AppendTextHeader(textSections);
                await writer.Append(HTMLTags.EndHeader);
                await AppendFigures(textSections);
                await AddTextTabs(idRanges, Ordinals.first, textSections);
                await writer.Append(HTMLTags.EndDiv);
                await AddScriptureTextToTabs(idRanges, Ordinals.first, textSections);
            }
            else
            {
                await AppendTextHeader(textSections);
                await AppendCitationReference(idRanges[Ordinals.first]);
                await writer.Append(HTMLTags.EndHeader);
                await AppendFigures(textSections);
                await writer.Append(HTMLTags.EndDiv);
                await AddScriptureText(idRanges[Ordinals.first], textSections);
            }
            await StartCommentSection(textSections);
        }

        internal void SetHeading(string paragraph)
        {
            paragraphs = new List<string>() { paragraph };
        }

        private async Task AppendFigures(TextSections textSections)
        {
            if (!textSections.ReadingView) return;
            await figureFormatter.GroupPictures(paragraphs);
        }

        private async Task AppendCitationReference((int start, int end) range)
        {
            Citation citation = new Citation(range.start, range.end);
            await writer.Append(" (");
            await CitationConverter.ToString(citation, writer);
            await writer.Append(")");
        }

        private async Task AddScriptureText((int start, int end) range, TextSections textSections)
        {
            Citation citation = new Citation(range.start, range.end);
            await writer.Append(HTMLTags.StartSectionWithClass +
                HTMLClasses.scriptureText +
                HTMLTags.CloseQuote);
            if (textSections.ReadingView)
            {
                await writer.Append(HTMLTags.OnClick +
                "ExpandReadingViewText(event)");
            }
            await writer.Append(HTMLTags.EndTag);
            List<Keyword> keywords = ReadKeywords(citation);
            formatter = new TextFormatter(writer);
            await formatter.AppendCitationData(citation);
            await writer.Append(HTMLTags.StartDivWithClass+
                HTMLClasses.scriptureQuote+
                HTMLTags.CloseQuoteEndTag);
            if (textSections.ReadingView)
                await AppendReadingViewKeywords(keywords, citation, textSections);
            else
                await AppendKeywords(keywords, citation, textSections);
            await writer.Append(HTMLTags.EndDiv +
                HTMLTags.StartDivWithClass +
                HTMLClasses.cleanquote +
                HTMLTags.CloseQuoteEndTag);
            await formatter.AppendCleanQuote(keywords);
            await writer.Append(HTMLTags.EndDiv+
                HTMLTags.EndSection);
        }

        private async Task AppendReadingViewKeywords(List<Keyword> keywords, Citation citation, TextSections textSections)
        {
            if (textSections.navigating)
            {
                await formatter.AppendReadingViewKeywords(keywords, citation);
                return;
            }
            await formatter.AppendReadingViewKeywords(keywords, citation, textSections.sourceCitation); 
        }

        private async Task AppendKeywords(List<Keyword> keywords, Citation citation, TextSections textSection)
        {
            if (textSection.navigating)
            {
                await formatter.AppendKeywords(keywords, citation);
                return;
            }
            await formatter.AppendKeywords(keywords, citation, textSection.sourceCitation);
        }

        public static async Task<List<(int start, int end)>> ReadLinks(int commentID)
        {
            var reader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadCommentLinks),
                commentID);
            List<(int start, int end)> results = await reader.GetData<int, int>();
            reader.Close();
            return results;
        }
        public static List<Keyword> ReadKeywords(Citation citation)
        {
            return ReadKeywords(
                citation.CitationRange.StartID.ID, citation.CitationRange.EndID.ID);
        }

        public static List<Keyword> ReadKeywords(int start, int end)
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadKeywords),
                start, end);
            var result = reader.GetClassData<Keyword>();
            reader.Close();
            return result;
        }


        public static List<string> ReadParagraphs(int commentID)
        {
            var reader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadComment),
                commentID);
            var results = reader.GetData<string>();
            reader.Close();
            return results;
        }

        private async Task AddTextTabs(List<(int start, int end)> idRanges, int sectionIndex, TextSections textSections)
        {
            await writer.Append(
                HTMLTags.StartList +
                HTMLTags.ID +
                HTMLClasses.tabs);
            await writer.Append(sectionIndex);
            await writer.Append(HTMLTags.Class +
                HTMLClasses.tabs+
                HTMLTags.CloseQuoteEndTag);
            int activeIndex = GetActiveIndex(idRanges, textSections);
            for (int i = Ordinals.first; i < idRanges.Count; i++)
            {
                Citation range = new Citation(idRanges[i].start, idRanges[i].end);
                await writer.Append(HTMLTags.StartListItem +
                    HTMLTags.ID +
                    HTMLClasses.tabs);
                await writer.Append(sectionIndex);
                await writer.Append('-');
                await writer.Append(i);
                if (i==activeIndex)
                {
                    await writer.Append(HTMLTags.Class +
                        HTMLClasses.active +
                        HTMLTags.CloseQuote);
                }
                await writer.Append(HTMLTags.OnClick +
                    JavaScriptFunctions.HandleTabClick +
                    HTMLTags.EndTag);
                await CitationConverter.ToString(range, writer);
                await writer.Append(HTMLTags.EndListItem);
            }
            await writer.Append(HTMLTags.EndList);
        }

        private async Task AppendTextHeader(TextSections textSections)
        {
            await writer.Append(HTMLTags.StartDivWithID +
                "header");
            await writer.Append(headerCount);
            await writer.Append(HTMLTags.CloseQuote +
                Symbol.space +
                HTMLClasses.dataComment);
            await writer.Append(headerCount);
            headerCount++;
            await writer.Append(Symbol.space+
                HTMLTags.Class+
                HTMLClasses.marker +
                HTMLTags.CloseQuoteEndTag+
                HTMLTags.EndDiv +
                HTMLTags.StartSectionWithClass +
                HTMLClasses.scriptureSection +
                HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartDivWithClass +
                HTMLClasses.scriptureHeader);
            if (textSections.ReadingView)
            {
                await writer.Append(HTMLTags.CloseQuoteEndTag+
                    HTMLTags.StartDivWithClass+
                    HTMLClasses.clear+
                    HTMLTags.CloseQuoteEndTag+
                    HTMLTags.EndDiv+
                    "<h3" +
                    HTMLTags.OnClick +
                    "HandleScriptureHeaderClicks()" +
                    HTMLTags.EndTag);
            }
            else
            {
                await writer.Append(Symbol.space+HTMLClasses.visible +
                    HTMLTags.CloseQuoteEndTag +
                    HTMLTags.StartHeader);
            }
            await writer.Append(paragraphs[Ordinals.first][Ordinals.third..Ordinals.nexttolast]);
        }

        private async Task AddScriptureTextToTabs(List<(int start, int end)> idRanges, int sectionIndex, TextSections textSections)
        {
            await writer.Append(HTMLTags.StartList +
                HTMLTags.ID +
                HTMLClasses.tabs);
            await writer.Append(sectionIndex);
            await writer.Append(HTMLClasses.tabSuffix +
                HTMLTags.Class +
                HTMLClasses.tab +
                HTMLTags.CloseQuoteEndTag);
            await AddScriptureTabs(idRanges, sectionIndex, textSections);
            await writer.Append(HTMLTags.EndList);
        }

        private async Task AddScriptureTabs(List<(int start, int end)> idRanges, int sectionIndex, TextSections textSections)
        {
            int activeIndex = GetActiveIndex(idRanges, textSections);
            for (int i = Ordinals.first; i < idRanges.Count; i++)
            {
                await writer.Append(HTMLTags.StartListItem +
                    HTMLTags.ID +
                    HTMLClasses.tabs);
                await writer.Append(sectionIndex);
                await writer.Append('-');
                await writer.Append(i);
                await writer.Append(HTMLClasses.tabSuffix);
                await writer.Append(HTMLTags.Class);
                Citation citation = new Citation(idRanges[i].start, idRanges[i].end);
                if (i==activeIndex)
                {
                    await writer.Append(HTMLClasses.active);
                }
                await writer.Append(HTMLClasses.rangeData +
                    HTMLTags.CloseQuote +
                    HTMLClasses.dataStart);
                await writer.Append(idRanges[i].start);
                await writer.Append(HTMLClasses.dataEnd);
                await writer.Append(idRanges[i].end);
                await writer.Append(HTMLTags.EndTag +
                    HTMLTags.StartSectionWithClass +
                    HTMLClasses.scriptureText +
                    HTMLTags.CloseQuote);
                if (textSections.ReadingView)
                {
                    await writer.Append(HTMLTags.OnClick +
                    "ExpandReadingViewText(event)");
                }
                await writer.Append(HTMLTags.EndTag);
                List<Keyword> keywords = ReadKeywords(citation);
                formatter = new TextFormatter(writer);
                await writer.Append(HTMLTags.StartDivWithClass +
                    HTMLClasses.scriptureQuote +
                    HTMLTags.CloseQuoteEndTag);
                if (textSections.ReadingView)
                    await AppendReadingViewKeywords(keywords, citation, textSections);
                else
                    await AppendKeywords(keywords, citation, textSections);
                await writer.Append(HTMLTags.EndDiv);
                await writer.Append(HTMLTags.StartDivWithClass +
                    HTMLClasses.cleanquote +
                    HTMLTags.CloseQuoteEndTag);
                await formatter.AppendCleanQuote(keywords);
                await writer.Append(HTMLTags.EndDiv +
                    HTMLTags.EndSection +
                    HTMLTags.EndListItem);
            }
        }

        private int GetActiveIndex(List<(int start, int end)> idRanges, TextSections textSections)
        {
            for (int i = Ordinals.first; i < idRanges.Count; i++)
            {
                Citation range = new Citation(idRanges[i].start, idRanges[i].end);
                if ((range.CitationRange.Contains(textSections.sourceCitation.CitationRange)) ||
                  (textSections.sourceCitation.CitationRange.Contains(range.CitationRange)) ||
                  ((range.CitationRange.Book == textSections.sourceCitation.CitationRange.Book) &&
                  (range.CitationRange.FirstChapter == textSections.sourceCitation.CitationRange.FirstChapter)))
                    return i;
            }
            return Ordinals.first;
        }

        private async Task AddComment(TextSections textSections)
        {
            await StartCommentSection(textSections);
            if (textSections.targetCitation != null) Parser.SetTargetRange(textSections.targetCitation.CitationRange);
            for (int i = Ordinals.second; i < paragraphs.Count; i++)
            {
                await Parser.ParseParagraph(paragraphs[i], i);
            }
            await EndCommentSection();
        }

        public async Task ParseParagraph(string paragraph, int paragraphIndex)
        {
            await Parser.ParseParagraph(paragraph, paragraphIndex);
        }

        public async Task EndCommentSection()
        {
            await writer.Append(HTMLTags.EndSection + HTMLTags.EndSection);
        }

        public async Task StartCommentSection(TextSections textSections)
        {
            await AddCommentHeading(textSections);
            await writer.Append(HTMLTags.StartSectionWithClass +
                HTMLClasses.scriptureComment);
            if (textSections.ReadingView)
            {
                await writer.Append(Symbol.space + HTMLClasses.hidden);
            }
            await writer.Append(HTMLTags.CloseQuoteEndTag);
            Parser.SetStartHTML(HTMLTags.StartParagraphWithClass + HTMLClasses.comment +
                HTMLTags.CloseQuoteEndTag);
            Parser.SetEndHTML(HTMLTags.EndParagraph);
        }

        private async Task AddCommentHeading(TextSections textSections)
        {
            if (!textSections.ReadingView) return;
            await writer.Append(
                HTMLTags.StartDivWithClass +
                HTMLClasses.scriptureCommentHeader +
                Symbol.space+
                HTMLClasses.hidden +
                HTMLTags.CloseQuote+
                HTMLTags.OnClick+
                "HandleCommentHeaderClicks()"+
                HTMLTags.EndTag +
                HTMLTags.StartHeader);
            await writer.Append(paragraphs[Ordinals.first][Ordinals.third..Ordinals.nexttolast]);
            await writer.Append(HTMLTags.EndHeader +
                HTMLTags.EndDiv);
        }
    }
}
