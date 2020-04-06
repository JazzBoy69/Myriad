using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myriad.Data;
using Myriad.Parser;
using Myriad.Library;

namespace Myriad.Pages
{
    public class TextSection
    {
        List<string> paragraphs;
        readonly HTMLWriter writer;
        TextFormatter formatter;
        readonly PageParser parser;
        bool activeSet;
        Citation sourceCitation;
        //todo cache sections?
        public TextSection(HTMLWriter writer)
        {
            this.writer = writer;
            parser = new PageParser(writer);
        }
        public async Task AddReadingViewSection(int commentID)
        {
            throw new NotImplementedException();
            activeSet = false;
            //todo reading view
        }

        public async Task AddTextSection(int commentID, Citation sourceCitation)
        {
            activeSet = false;
            this.sourceCitation = sourceCitation;
            List<(int start, int end)> idRanges = ReadLinks(commentID);

            parser.SetParagraphInfo(ParagraphType.Comment, commentID);
            paragraphs = ReadParagraphs(commentID);
            if (idRanges.Count > 1)
            {
                await AddTextTabs(idRanges, Ordinals.first);
                await AddScriptureTextToTabs(idRanges, Ordinals.first);
            }
            else await AddSingleText(idRanges[Ordinals.first]);
            await AddComment();
        }

        private async Task AddSingleText((int start, int end) textRange)
        {
            await writer.Append(HTMLTags.StartSectionWithClass+
                HTMLClasses.scriptureSection+
                HTMLTags.CloseQuoteEndTag+
                HTMLTags.StartHeader);
            await writer.Append(paragraphs[Ordinals.first][Ordinals.third..Ordinals.nexttolast]);
            Citation citation = new Citation(textRange.start, textRange.end);
            await writer.Append(" (");
            await CitationConverter.ToString(citation, writer);
            await writer.Append(")"+
                HTMLTags.EndHeader);
            await AddScriptureText(citation);
        }

        private async Task AddScriptureText(Citation citation)
        {
            await writer.Append(HTMLTags.StartSectionWithClass+
                HTMLClasses.scriptureText+
                HTMLTags.CloseQuoteEndTag);
            List<Keyword> keywords = ReadKeywords(citation); //todo db async
            formatter = new TextFormatter(writer);
            await formatter.AppendCitationData(citation);
            await writer.Append(HTMLTags.StartDivWithClass+
                HTMLClasses.scriptureQuote+
                HTMLTags.CloseQuoteEndTag);
            await formatter.AppendKeywords(keywords);
            await writer.Append(HTMLTags.EndDiv+
                HTMLTags.EndSection);
        }
        public List<(int start, int end)> ReadLinks(int commentID)
        {
            var reader = DataReaderProvider<int>.Reader(DataOperation.ReadCommentLinks,
                commentID);
            List<(int start, int end)> results = reader.GetData<int, int>();
            reader.Close();
            return results;
        }
        public List<Keyword> ReadKeywords(Citation citation)
        {
            var reader = DataReaderProvider<int, int>.Reader(DataOperation.ReadKeywords,
                citation.CitationRange.StartID, citation.CitationRange.EndID);
            return reader.GetClassData<Keyword>();
        }


        public List<string> ReadParagraphs(int commentID)
        {
            var reader = DataReaderProvider<int>.Reader(DataOperation.ReadComment,
                commentID);
            var results = reader.GetData<string>();
            reader.Close();
            return results;
        }

        private async Task AddTextTabs(List<(int start, int end)> idRanges, int index)
        {
            await writer.Append(HTMLTags.StartSectionWithClass+
                HTMLClasses.scriptureSection+
                HTMLTags.CloseQuoteEndTag+
                HTMLTags.StartHeader);
            await writer.Append(paragraphs[Ordinals.first][Ordinals.third..Ordinals.nexttolast]);
            await writer.Append(HTMLTags.EndHeader+
                HTMLTags.StartList+
                HTMLTags.ID+
                HTMLClasses.tabs);
            await writer.Append(index);
            await writer.Append(HTMLTags.Class+
                HTMLClasses.tabs+
                HTMLTags.CloseQuoteEndTag);
            for (int i = Ordinals.first; i < idRanges.Count; i++)
            {
                Citation range = new Citation(idRanges[i].start, idRanges[i].end);
                await writer.Append(HTMLTags.StartListItem+
                    HTMLTags.ID+
                    HTMLClasses.tabs);
                await writer.Append(index);
                await writer.Append('-');
                await writer.Append(i);
                if ((!activeSet) && ((range.CitationRange.Contains(sourceCitation.CitationRange)) ||
                    (sourceCitation.CitationRange.Contains(range.CitationRange)) || 
                    (range.CitationRange.Book == sourceCitation.CitationRange.Book)))
                {
                    await writer.Append(HTMLTags.Class+
                        HTMLClasses.active+
                        HTMLTags.CloseQuote);
                    activeSet = true;
                }
                await writer.Append(HTMLTags.OnClick+
                    JavaScriptFunctions.HandleTabClick+
                    HTMLTags.EndTag);
                await CitationConverter.ToString(range, writer);
                await writer.Append(HTMLTags.EndListItem);
            }
            await writer.Append(HTMLTags.EndList);
        }


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
                await writer.Append(HTMLTags.EndTag+
                    HTMLTags.StartSectionWithClass+
                    HTMLClasses.scriptureText+
                    HTMLTags.CloseQuoteEndTag);
                List<Keyword> keywords = ReadKeywords(range);
                formatter = new TextFormatter(writer);
                await writer.Append(HTMLTags.StartDivWithClass+
                    HTMLClasses.scriptureQuote+
                    HTMLTags.CloseQuoteEndTag);
                await formatter.AppendKeywords(keywords);
                await writer.Append(HTMLTags.EndDiv+
                    HTMLTags.EndSection+
                    HTMLTags.EndListItem);
            }
            await writer.Append(HTMLTags.EndList);
        }
        private async Task AddComment()
        {
            parser.SetStartHTML(HTMLTags.StartSectionWithClass+ HTMLClasses.scriptureComment+ HTMLTags.CloseQuoteEndTag);
            parser.SetEndHTML(HTMLTags.EndSection + HTMLTags.EndSection);
            for (int i = Ordinals.second; i < paragraphs.Count; i++)
            {
                await parser.ParseParagraph(paragraphs[i], i);
            }
        }
    }
}
