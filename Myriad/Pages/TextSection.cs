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

        public TextSection(HTMLResponse builder)
        {
            this.builder = builder;
            parser = new CommentParser(builder);
            parser.SetParagraphCreator(new MarkedUpParagraphCreator());
        }
        public void AddReadingViewSection(int commentID)
        {
            throw new NotImplementedException();
            //todo reading view
        }

        public void AddTextSection(int commentID)
        {
            List<(int start, int end)> idRanges = ReadLinks(commentID);

            paragraphs = ReadParagraphs(commentID);
            if (idRanges.Count > 1) AddTextTabs(idRanges);
            else AddText(idRanges[Ordinals.first]);
            AddComment();
        }
        private void AddText((int start, int end) textRange)
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
        private void AddTextTabs(List<(int start, int end)> idRanges)
        {
            throw new NotImplementedException();
            // todo parallel texts
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
