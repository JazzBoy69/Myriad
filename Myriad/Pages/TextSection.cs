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
        HTMLWriter writer;
        TextFormatter formatter;
        PageParser parser;
        bool activeSet;
        Citation sourceCitation;
        //todo cache sections?
        public TextSection(HTMLWriter writer)
        {
            this.writer = writer;
            parser = new PageParser(writer);
        }
        public void AddReadingViewSection(int commentID)
        {
            throw new NotImplementedException();
            activeSet = false;
            //todo reading view
        }

        public void AddTextSection(int commentID, Citation sourceCitation)
        {
            activeSet = false;
            this.sourceCitation = sourceCitation;
            List<(int start, int end)> idRanges = ReadLinks(commentID);

            parser.SetParagraphInfo(ParagraphType.Comment, commentID);
            paragraphs = ReadParagraphs(commentID);
            if (idRanges.Count > 1)
            {
                AddTextTabs(idRanges, Ordinals.first);
                AddScriptureTextToTabs(idRanges, Ordinals.first);
            }
            else AddSingleText(idRanges[Ordinals.first]);
            AddComment();
        }

        private void AddSingleText((int start, int end) textRange)
        {
            writer.StartSectionWithClass(HTMLClasses.scriptureSection);
            writer.Append(HTMLTags.StartHeader);
            writer.Append(paragraphs[Ordinals.first].Substring(Ordinals.third,
                paragraphs[Ordinals.first].Length - 4));
            Citation citation = new Citation(textRange.start, textRange.end);
            writer.Append(" (");
            writer.Append(CitationConverter.ToString(citation));
            writer.Append(")");
            writer.Append(HTMLTags.EndHeader);
            AddScriptureText(citation);
        }

        private void AddScriptureText(Citation citation)
        {
            writer.StartSectionWithClass(HTMLClasses.scriptureText);
            List<Keyword> keywords = ReadKeywords(citation);
            formatter = new TextFormatter(writer);
            formatter.AppendCitationData(citation);
            writer.StartDivWithClass(HTMLClasses.scriptureQuote);
            formatter.AppendKeywords(keywords);
            writer.Append(HTMLTags.EndDiv);
            writer.Append(HTMLTags.EndSection);
        }
        public List<(int start, int end)> ReadLinks(int commentID)
        {
            var reader = SQLServerReaderProvider<int>.Reader(DataOperation.ReadCommentLinks,
                commentID);
            return reader.GetData<int, int>();
        }
        public List<Keyword> ReadKeywords(Citation citation)
        {
            var reader = SQLServerReaderProvider<int, int>.Reader(DataOperation.ReadKeywords,
                citation.CitationRange.StartID, citation.CitationRange.EndID);
            return reader.GetClassData<Keyword>();
        }


        public List<string> ReadParagraphs(int commentID)
        {
            var reader = SQLServerReaderProvider<int>.Reader(DataOperation.ReadComment,
                commentID);
            return reader.GetData<string>();
        }

        private void AddTextTabs(List<(int start, int end)> idRanges, int index)
        {
            writer.StartSectionWithClass(HTMLClasses.scriptureSection);
            writer.Append(HTMLTags.StartHeader);
            writer.Append(paragraphs[Ordinals.first].Substring(Ordinals.third,
                paragraphs[Ordinals.first].Length - 4));
            writer.Append(HTMLTags.EndHeader);

            writer.Append(HTMLTags.StartList);
            writer.Append(HTMLTags.ID);
            writer.Append(HTMLClasses.tabs);
            writer.Append(index);
            writer.Append(HTMLTags.Class);
            writer.Append(HTMLClasses.tabs);
            writer.Append(HTMLTags.CloseQuoteEndTag);
            for (int i = Ordinals.first; i < idRanges.Count; i++)
            {
                Citation range = new Citation(idRanges[i].start, idRanges[i].end);
                writer.Append(HTMLTags.StartListItem);
                writer.Append(HTMLTags.ID);
                writer.Append(HTMLClasses.tabs);
                writer.Append(index);
                writer.Append('-');
                writer.Append(i);
                if ((!activeSet) && ((range.CitationRange.Contains(sourceCitation.CitationRange)) ||
                    (sourceCitation.CitationRange.Contains(range.CitationRange)) || 
                    (range.CitationRange.Book == sourceCitation.CitationRange.Book)))
                {
                    writer.Append(HTMLTags.Class);
                    writer.Append(HTMLClasses.active);
                    writer.Append(HTMLTags.CloseQuote);
                    activeSet = true;
                }
                writer.Append(HTMLTags.OnClick);
                writer.Append(JavaScriptFunctions.HandleTabClick);
                writer.Append(HTMLTags.EndTag);
                writer.Append(CitationConverter.ToString(range));
                writer.Append(HTMLTags.EndListItem);
            }
            writer.Append(HTMLTags.EndList);
        }


        private void AddScriptureTextToTabs(List<(int start, int end)> idRanges, int index)
        {
            writer.Append(HTMLTags.StartList);
            writer.Append(HTMLTags.ID);
            writer.Append(HTMLClasses.tabs);
            writer.Append(index);
            writer.Append(HTMLClasses.tabSuffix);
            writer.Append(HTMLTags.Class);
            writer.Append(HTMLClasses.tab);
            writer.Append(HTMLTags.CloseQuoteEndTag);
            for (int i = Ordinals.first; i < idRanges.Count; i++)
            {
                activeSet = false;
                writer.Append(HTMLTags.StartListItem);
                writer.Append(HTMLTags.ID);
                writer.Append(HTMLClasses.tabs);
                writer.Append(index);
                writer.Append('-');
                writer.Append(i);
                writer.Append(HTMLClasses.tabSuffix);
                writer.Append(HTMLTags.Class);
                Citation range = new Citation(idRanges[i].start, idRanges[i].end);
                if ((!activeSet) && ((range.CitationRange.Contains(sourceCitation.CitationRange)) ||
                    (sourceCitation.CitationRange.Contains(range.CitationRange)) ||
                    (range.CitationRange.Book == sourceCitation.CitationRange.Book)))
                {
                    writer.Append(HTMLClasses.active);
                    activeSet = true;
                }
                writer.Append(HTMLClasses.rangeData);
                writer.Append(HTMLTags.CloseQuote);
                writer.Append(HTMLClasses.dataStart);
                writer.Append(idRanges[i].start);
                writer.Append(HTMLClasses.dataEnd);
                writer.Append(idRanges[i].end);
                writer.Append(HTMLTags.EndTag);

                writer.StartSectionWithClass(HTMLClasses.scriptureText);
                List<Keyword> keywords = ReadKeywords(range);
                formatter = new TextFormatter(writer);
                writer.StartDivWithClass(HTMLClasses.scriptureQuote);
                formatter.AppendKeywords(keywords);
                writer.Append(HTMLTags.EndDiv);
                writer.Append(HTMLTags.EndSection);

                writer.Append(HTMLTags.EndListItem);
            }
            writer.Append(HTMLTags.EndList);
        }
        private void AddComment()
        {
            writer.StartSectionWithClass(HTMLClasses.scriptureComment);
            for (int i = Ordinals.first; i < paragraphs.Count; i++)
            {
                parser.ParseParagraph(paragraphs[i], i);
            }
            writer.Append(HTMLTags.EndSection);
            writer.Append(HTMLTags.EndSection);
        }
    }
}
