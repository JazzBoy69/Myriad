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
        HTMLResponse builder;
        TextFormatter formatter;
        CommentParser parser;
        bool activeSet;
        Citation sourceCitation;
        //todo cache sections?
        public TextSection(HTMLResponse builder)
        {
            this.builder = builder;
            parser = new CommentParser(builder);
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
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
            builder.StartSectionWithClass(HTMLClasses.scriptureSection);
            builder.Append(HTMLTags.StartHeader);
            builder.Append(paragraphs[Ordinals.first].Substring(Ordinals.third,
                paragraphs[Ordinals.first].Length - 4));
            Citation citation = new Citation(textRange.start, textRange.end);
            builder.Append(" (");
            builder.Append(CitationConverter.ToString(citation));
            builder.Append(")");
            builder.Append(HTMLTags.EndHeader);
            AddScriptureText(citation);
        }

        private void AddScriptureText(Citation citation)
        {
            builder.StartSectionWithClass(HTMLClasses.scriptureText);
            List<Keyword> keywords = ReadKeywords(citation);
            formatter = new TextFormatter(builder);
            formatter.AppendCitationData(citation);
            builder.StartDivWithClass(HTMLClasses.scriptureQuote);
            formatter.AppendKeywords(keywords);
            builder.Append(HTMLTags.EndDiv);
            //todo edit comment link in header
            builder.Append(HTMLTags.EndSection);
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
            var reader = SQLServerReaderProvider<int>.Reader(DataOperation.ReadCommentParagraphs,
                commentID);
            return reader.GetData<string>();
        }

        private void AddTextTabs(List<(int start, int end)> idRanges, int index)
        {
            //todo refactor heading?
            builder.StartSectionWithClass(HTMLClasses.scriptureSection);
            builder.Append(HTMLTags.StartHeader);
            builder.Append(paragraphs[Ordinals.first].Substring(Ordinals.third,
                paragraphs[Ordinals.first].Length - 4));
            builder.Append(HTMLTags.EndHeader);

            builder.Append(HTMLTags.StartList);
            builder.Append(HTMLTags.ID);
            builder.Append(HTMLClasses.tabs);
            builder.Append(index);
            builder.Append(HTMLTags.Class);
            builder.Append(HTMLClasses.tabs);
            builder.Append(HTMLTags.CloseQuoteEndTag);
            for (int i = Ordinals.first; i < idRanges.Count; i++)
            {
                Citation range = new Citation(idRanges[i].start, idRanges[i].end);
                builder.Append(HTMLTags.StartListItem);
                builder.Append(HTMLTags.ID);
                builder.Append(HTMLClasses.tabs);
                builder.Append(index);
                builder.Append('-');
                builder.Append(i);
                if ((!activeSet) && ((range.CitationRange.Contains(sourceCitation.CitationRange)) ||
                    (sourceCitation.CitationRange.Contains(range.CitationRange)) || 
                    (range.CitationRange.Book == sourceCitation.CitationRange.Book)))
                {
                    builder.Append(HTMLTags.Class);
                    builder.Append(HTMLClasses.active);
                    builder.Append(HTMLTags.CloseQuote);
                    activeSet = true;
                }
                builder.Append(HTMLTags.EndTag);
                builder.Append(CitationConverter.ToString(range));
                builder.Append(HTMLTags.EndListItem);
            }
            builder.Append(HTMLTags.EndList);
        }


        private void AddScriptureTextToTabs(List<(int start, int end)> idRanges, int index)
        {
            builder.Append(HTMLTags.StartList);
            builder.Append(HTMLTags.ID);
            builder.Append(HTMLClasses.tabs);
            builder.Append(index);
            builder.Append(HTMLClasses.tabSuffix);
            builder.Append(HTMLTags.Class);
            builder.Append(HTMLClasses.tab);
            builder.Append(HTMLTags.CloseQuoteEndTag);
            for (int i = Ordinals.first; i < idRanges.Count; i++)
            {
                activeSet = false;
                builder.Append(HTMLTags.StartListItem);
                builder.Append(HTMLTags.ID);
                builder.Append(index);
                builder.Append('-');
                builder.Append(i);
                builder.Append(HTMLClasses.tabSuffix);
                Citation range = new Citation(idRanges[i].start, idRanges[i].end);
                if ((!activeSet) && ((range.CitationRange.Contains(sourceCitation.CitationRange)) ||
                    (sourceCitation.CitationRange.Contains(range.CitationRange)) ||
                    (range.CitationRange.Book == sourceCitation.CitationRange.Book)))
                {
                    builder.Append(HTMLTags.Class);
                    builder.Append(HTMLClasses.active);
                    builder.Append(HTMLTags.CloseQuote);
                    activeSet = true;
                }
                builder.Append(HTMLClasses.rangeData);
                builder.Append(HTMLTags.dataStart);
                builder.Append(idRanges[i].start);
                builder.Append(HTMLTags.dataEnd);
                builder.Append(idRanges[i].end);
                builder.Append(HTMLTags.EndTag);

                AddScriptureText(range);

                builder.Append(HTMLTags.EndListItem);
            }
            builder.Append(HTMLTags.EndList);
        }
        private void AddComment()
        {
            paragraphs.RemoveAt(Ordinals.first);
            builder.StartSectionWithClass(HTMLClasses.scriptureComment);
            parser.Parse(paragraphs);
            builder.Append(HTMLTags.EndSection);
            builder.Append(HTMLTags.EndSection);
        }
    }
}
