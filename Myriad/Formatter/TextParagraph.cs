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
    public class TextParagraph
    {

        internal static async Task AddText(HTMLWriter writer, Citation citation, bool navigating)
        {
            int paragraphIndex = await ReadParagraphIndex(citation.CitationRange.StartID.ID);
            await writer.Append(HTMLTags.StartDivWithClass +
                HTMLClasses.paragraphText +
                HTMLTags.CloseQuote);
            List<(int start, int end)> paragraphRanges = await ReadParagraghRanges(citation);
            for (int index = Ordinals.first; index<paragraphRanges.Count; index++)
            {
                await AddScriptureParagraph(writer, paragraphRanges[index], citation, navigating);
            }

            await writer.Append(HTMLTags.EndDiv);
        }



        private static async Task AddScriptureParagraph(HTMLWriter writer, (int start, int end) paragraphRange, Citation sourceCitation, bool navigating)
        {
            await writer.Append(HTMLTags.StartSectionWithClass +
                HTMLClasses.scriptureSection +
                HTMLTags.CloseQuoteEndTag + 
                HTMLTags.StartSectionWithClass +
                HTMLClasses.scriptureText +
                HTMLTags.CloseQuote);

            await writer.Append(HTMLTags.OnClick +
                "ExpandReadingViewText(event)");

            await writer.Append(HTMLTags.EndTag);
            
            await writer.Append(HTMLTags.StartDivWithClass +
                HTMLClasses.scriptureQuote +
                HTMLTags.CloseQuoteEndTag);
            await AppendParagraphKeywords(writer, paragraphRange, sourceCitation, navigating);
            await writer.Append(HTMLTags.EndDiv +
                HTMLTags.EndSection +
                HTMLTags.EndSection);
        }

        private static async Task AppendParagraphKeywords(HTMLWriter writer, (int start, int end) paragraphRange, Citation sourceCitation, bool navigating)
        {
            var commentIDs = GetCommentIDs(paragraphRange.start, paragraphRange.end);
            for (int index = Ordinals.first; index < commentIDs.Count; index++)
            {
                List<(int start, int end)> idRanges = await ReadLinks(commentIDs[index]);
                for (int i = Ordinals.first; i < idRanges.Count; i++)
                {
                    if ((idRanges[i].start > paragraphRange.end) || (idRanges[i].end < paragraphRange.start)) continue;
                    await AppendSpanKeywords(writer, idRanges[i], index, sourceCitation, navigating);
                    break;
                }
            }
        }

        private static async Task AppendSpanKeywords(HTMLWriter writer, (int start, int end) range, int index, Citation sourceCitation, bool navigating) 
        {
            Citation citation = new Citation(range.start, range.end);
            TextFormatter formatter = new TextFormatter(writer);
            List<Keyword> keywords = ReadKeywords(citation);
            if (navigating)
            {
                await formatter.AppendCommentSpanKeywords(keywords, citation, index);
                return;
            }
            await formatter.AppendCommentSpanKeywords(keywords, citation, sourceCitation, index);
        }

        private static List<Keyword> ReadKeywords(Citation citation)
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadKeywords),
                citation.CitationRange.StartID.ID, citation.CitationRange.EndID.ID);
            var result = reader.GetClassData<Keyword>();
            reader.Close();
            return result;
        }
        private static async Task<int> ReadParagraphIndex(int start)
        {
            var reader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadParagraphIndex),
                start);
            int result = await reader.GetDatum<int>();
            reader.Close();
            return result;
        }

        private static async Task<List<(int start, int end)>> ReadParagraghRanges(Citation citation)
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadParagraphRanges),
                citation.CitationRange.StartID.ID, citation.CitationRange.EndID.ID);
            List<(int, int)> result = await reader.GetData<int, int>();
            reader.Close();
            return result;
        }

        private static List<int> GetCommentIDs(int start, int end)
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadCommentIDs),
                start, end);
            var results = reader.GetData<int>();
            reader.Close();
            return results;
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
    }
}
