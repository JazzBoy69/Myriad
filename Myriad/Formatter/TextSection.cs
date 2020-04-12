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
        readonly PageParser parser;
        readonly FigureFormatter figureFormatter;
        bool activeSet;
        Citation sourceCitation;
        bool readingView;
        int headerCount = Ordinals.first;

        public TextSectionFormatter(HTMLWriter writer)
        {
            this.writer = writer;
            parser = new PageParser(writer);
            figureFormatter = new FigureFormatter(writer);
        }

        public async Task AddTextSection(List<int> commentIDs, int index, Citation sourceCitation, bool readingView)
        {
            activeSet = false;
            this.readingView = readingView;
            this.sourceCitation = sourceCitation;
            List<(int start, int end)> idRanges = await ReadLinks(commentIDs[index]);

            parser.SetParagraphInfo(ParagraphType.Comment, commentIDs[index]);
            paragraphs = await ReadParagraphs(commentIDs[index]);
            if (idRanges.Count > 1)
            {
                await AppendTextHeader();
                await writer.Append(HTMLTags.EndHeader);
                await AppendFigures();
                await AddTextTabs(idRanges, index);
                await writer.Append(HTMLTags.EndDiv);
                await AddScriptureTextToTabs(idRanges, index);
            }
            else
            {
                await AppendTextHeader();
                await AppendCitationReference(idRanges[Ordinals.first]);
                await writer.Append(HTMLTags.EndHeader);
                await AppendFigures(); 
                await writer.Append(HTMLTags.EndDiv);
                await AddScriptureText(idRanges[Ordinals.first]);
            }
            await AddComment();
        }

        private async Task AppendFigures()
        {
            if (!readingView) return;
            await figureFormatter.GroupPictures(paragraphs);
        }

        private async Task AppendCitationReference((int start, int end) range)
        {
            Citation citation = new Citation(range.start, range.end);
            await writer.Append(" (");
            await CitationConverter.ToString(citation, writer);
            await writer.Append(")");
        }

        private async Task AddScriptureText((int start, int end) range)
        {
            Citation citation = new Citation(range.start, range.end);
            await writer.Append(HTMLTags.StartSectionWithClass +
                HTMLClasses.scriptureText +
                HTMLTags.CloseQuote);
            if (readingView)
            {
                await writer.Append(HTMLTags.OnClick +
                "ExpandReadingViewText(event)");
            }
            await writer.Append(HTMLTags.EndTag);
            List<Keyword> keywords = await ReadKeywords(citation); //todo db async
            formatter = new TextFormatter(writer);
            await formatter.AppendCitationData(citation);
            await writer.Append(HTMLTags.StartDivWithClass+
                HTMLClasses.scriptureQuote+
                HTMLTags.CloseQuoteEndTag);
            await formatter.AppendKeywords(keywords, readingView);
            await writer.Append(HTMLTags.EndDiv+
                HTMLTags.EndSection);
        }
        public async Task<List<(int start, int end)>> ReadLinks(int commentID)
        {
            var reader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadCommentLinks),
                commentID);
            List<(int start, int end)> results = await reader.GetData<int, int>();
            reader.Close();
            return results;
        }
        public async Task<List<Keyword>> ReadKeywords(Citation citation)
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadKeywords),
                citation.CitationRange.StartID, citation.CitationRange.EndID);
            return await reader.GetClassData<Keyword>();
        }


        public static async Task<List<string>> ReadParagraphs(int commentID)
        {
            var reader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadComment),
                commentID);
            var results = await reader.GetData<string>();
            reader.Close();
            return results;
        }

        private async Task AddTextTabs(List<(int start, int end)> idRanges, int index)
        {
            await writer.Append(
                HTMLTags.StartList +
                HTMLTags.ID +
                HTMLClasses.tabs);
            await writer.Append(index);
            await writer.Append(HTMLTags.Class +
                HTMLClasses.tabs+
                HTMLTags.CloseQuoteEndTag);
            for (int i = Ordinals.first; i < idRanges.Count; i++)
            {
                Citation range = new Citation(idRanges[i].start, idRanges[i].end);
                await writer.Append(HTMLTags.StartListItem +
                    HTMLTags.ID +
                    HTMLClasses.tabs);
                await writer.Append(index);
                await writer.Append('-');
                await writer.Append(i);
                if ((!activeSet) && ((range.CitationRange.Contains(sourceCitation.CitationRange)) ||
                    (sourceCitation.CitationRange.Contains(range.CitationRange)) ||
                    (range.CitationRange.Book == sourceCitation.CitationRange.Book)))
                {
                    await writer.Append(HTMLTags.Class +
                        HTMLClasses.active +
                        HTMLTags.CloseQuote);
                    activeSet = true;
                }
                await writer.Append(HTMLTags.OnClick +
                    JavaScriptFunctions.HandleTabClick +
                    HTMLTags.EndTag);
                await CitationConverter.ToString(range, writer);
                await writer.Append(HTMLTags.EndListItem);
            }
            await writer.Append(HTMLTags.EndList);
        }

        private async Task AppendTextHeader()
        {
            await writer.Append(HTMLTags.StartDivWithID +
                "header");
            await writer.Append(headerCount);
            headerCount++;
            await writer.Append(HTMLTags.CloseQuote+
                HTMLTags.Class+
                HTMLClasses.marker +
                HTMLTags.CloseQuoteEndTag+
                HTMLTags.EndDiv +
                HTMLTags.StartSectionWithClass +
                HTMLClasses.scriptureSection +
                HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartDivWithClass +
                HTMLClasses.scriptureHeader);
            if (readingView)
            {
                await writer.Append(HTMLTags.CloseQuoteEndTag+
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
        //todo refactor
        private async Task AddScriptureTextToTabs(List<(int start, int end)> idRanges, int index)
        {
            await writer.Append(HTMLTags.StartList+
                HTMLTags.ID+
                HTMLClasses.tabs);
            await writer.Append(index);
            await writer.Append(HTMLClasses.tabSuffix+
                HTMLTags.Class+
                HTMLClasses.tab+
                HTMLTags.CloseQuoteEndTag);
            for (int i = Ordinals.first; i < idRanges.Count; i++)
            {
                activeSet = false;
                await writer.Append(HTMLTags.StartListItem+
                    HTMLTags.ID+
                    HTMLClasses.tabs);
                await writer.Append(index);
                await writer.Append('-');
                await writer.Append(i);
                await writer.Append(HTMLClasses.tabSuffix);
                await writer.Append(HTMLTags.Class);
                Citation range = new Citation(idRanges[i].start, idRanges[i].end);
                if ((!activeSet) && ((range.CitationRange.Contains(sourceCitation.CitationRange)) ||
                    (sourceCitation.CitationRange.Contains(range.CitationRange)) ||
                    (range.CitationRange.Book == sourceCitation.CitationRange.Book)))
                {
                    await writer.Append(HTMLClasses.active);
                    activeSet = true;
                }
                await writer.Append(HTMLClasses.rangeData+
                    HTMLTags.CloseQuote+
                    HTMLClasses.dataStart);
                await writer.Append(idRanges[i].start);
                await writer.Append(HTMLClasses.dataEnd);
                await writer.Append(idRanges[i].end);
                await writer.Append(HTMLTags.EndTag +
                    HTMLTags.StartSectionWithClass +
                    HTMLClasses.scriptureText +
                    HTMLTags.CloseQuote);
                if (readingView)
                {
                    await writer.Append(HTMLTags.OnClick +
                    "ExpandReadingViewText(event)");
                }
                await writer.Append(HTMLTags.EndTag);
                List<Keyword> keywords = await ReadKeywords(range);
                formatter = new TextFormatter(writer);
                await writer.Append(HTMLTags.StartDivWithClass+
                    HTMLClasses.scriptureQuote+
                    HTMLTags.CloseQuoteEndTag);
                await formatter.AppendKeywords(keywords, readingView);
                await writer.Append(HTMLTags.EndDiv+
                    HTMLTags.EndSection+
                    HTMLTags.EndListItem);
            }
            await writer.Append(HTMLTags.EndList);
        }
        private async Task AddComment()
        {
            await AddCommentHeading();
            await writer.Append(HTMLTags.StartSectionWithClass +
                HTMLClasses.scriptureComment);
            if (readingView)
            {
                await writer.Append(Symbol.space + HTMLClasses.hidden);
            }
            await writer.Append(HTMLTags.CloseQuoteEndTag);
            parser.SetStartHTML(HTMLTags.StartParagraphWithClass + HTMLClasses.comment +
                HTMLTags.CloseQuoteEndTag);
            parser.SetEndHTML(HTMLTags.EndParagraph);
            for (int i = Ordinals.second; i < paragraphs.Count; i++)
            {
                await parser.ParseParagraph(paragraphs[i], i);
            }
            await writer.Append(HTMLTags.EndSection + HTMLTags.EndSection);
        }

        private async Task AddCommentHeading()
        {
            if (!readingView) return;
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
