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
        internal static async Task AddText(HTMLWriter writer, TextSections textSections)
        {
            await writer.Append(HTMLTags.StartDivWithID +
                HTMLClasses.paragraphText +
                HTMLTags.CloseQuoteEndTag);
            List<(int start, int end)> paragraphRanges = await DataRepository.ParagraphRanges(textSections.sourceCitation.Start, textSections.sourceCitation.End);
            for (int paragraphIndex = Ordinals.first; paragraphIndex<paragraphRanges.Count; paragraphIndex++)
            {
                await writer.Append(HTMLTags.StartSectionWithClass +
                HTMLClasses.scriptureSection +
                HTMLTags.CloseQuoteEndTag +
                    HTMLTags.StartDivWithClass +
                    HTMLClasses.clear +
                    HTMLTags.CloseQuoteEndTag
                    + HTMLTags.EndDiv);
                await AppendFigures(writer, paragraphRanges[paragraphIndex]);
                await AddScriptureParagraph(writer, paragraphRanges[paragraphIndex], textSections);
                await writer.Append(HTMLTags.EndSection);
            }

            await writer.Append(HTMLTags.EndDiv);
        }

        private async static Task AppendFigures(HTMLWriter writer, (int start, int end) range)
        {
            List<int> ids = await DataRepository.CommentIDsInRange(range.start, range.end);
            List<string> paragraphs = new List<string>();
            for (int i = Ordinals.first; i < ids.Count; i++)
            {
                paragraphs.AddRange(await TextSectionFormatter.ReadParagraphs(ids[i]));
            }
            FigureFormatter figureFormatter = new FigureFormatter(writer);
            await figureFormatter.GroupPictures(paragraphs);
        }

        private static async Task AddScriptureParagraph(HTMLWriter writer, (int start, int end) paragraphRange, 
            TextSections textSections)
        {
            await writer.Append(
                HTMLTags.StartSectionWithClass +
                HTMLClasses.scriptureText +
                HTMLTags.CloseQuote);

            await PageFormatter.AppendHandleParagraphClick(writer);

            await writer.Append(HTMLTags.EndTag);
            
            await writer.Append(HTMLTags.StartDivWithClass +
                HTMLClasses.scriptureQuote +
                HTMLTags.CloseQuoteEndTag);
            await AppendParagraphKeywords(writer, paragraphRange, textSections);
            await writer.Append(HTMLTags.EndDiv +
                HTMLTags.EndSection);
        }

        private static async Task AppendParagraphKeywords(HTMLWriter writer, (int start, int end) paragraphRange, 
            TextSections textSections)
        {
            var commentIDs = await DataRepository .CommentIDsInRange(paragraphRange.start, paragraphRange.end);
            for (int spanIndex = Ordinals.first; spanIndex < commentIDs.Count; spanIndex++)
            {
                await AppendSpan(writer, paragraphRange, textSections, commentIDs[spanIndex], spanIndex, commentIDs.Count>1);
            }
        }

        private static async Task AppendSpan(HTMLWriter writer, (int start, int end) paragraphRange, 
            TextSections textSections, int commentID, int spanIndex, bool multi)
        {
            await writer.Append(HTMLTags.StartSpanWithClass +
                HTMLClasses.commentMarker +
                HTMLTags.CloseQuote +
                HTMLTags.ID +
                HTMLClasses.markerID);
            await writer.Append(textSections.CommentIDs.IndexOf(commentID));
            await writer.Append(HTMLClasses.dataComment);
            await writer.Append(textSections.CommentIDs.IndexOf(commentID));
            await writer.Append(HTMLTags.EndTag);
            (int start, int end) idRange = await DataRepository.CommentLink(commentID, paragraphRange.start, paragraphRange.end);
            if (idRange.start < paragraphRange.start) idRange.start = paragraphRange.start;
            if (idRange.end > paragraphRange.end) idRange.end = paragraphRange.end;
            await AppendSpanKeywords(writer, idRange, spanIndex, textSections, multi);
            await writer.Append(HTMLTags.EndSpan);
        }

        private static async Task AppendSpanKeywords(HTMLWriter writer, (int start, int end) range, int index, 
            TextSections textSections, bool multi) 
        {
            Citation citation = new Citation(range.start, range.end);
            TextFormatter formatter = new TextFormatter(writer);
            List<Keyword> keywords = await DataRepository.RangeKeywords(citation.Start,
                citation.End);
            if (textSections.navigating)
            {
                await formatter.AppendCommentSpanKeywords(keywords, citation, index, multi);
                return;
            }
            await formatter.AppendCommentSpanKeywords(keywords, citation, textSections.highlightCitation, index, multi);
        }
    }
}
