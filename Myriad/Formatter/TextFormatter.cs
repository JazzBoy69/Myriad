using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.ResponseWriter;
using Myriad.Library;
using Myriad.Data;


namespace Myriad.Parser
{
    internal class TextFormatter
    {
        private readonly HTMLWriter writer;
        private bool poetic;
        public TextFormatter(HTMLWriter writer)
        {
            this.writer = writer;
        }

        public async Task AppendKeywords(List<Keyword> keywords, CitationRange range, CitationRange targetrange, bool navigating, bool readingView)
        {
            await AppendFirstWord(keywords, range, targetrange, navigating, readingView);
            for (int index = Ordinals.second; index < keywords.Count; index++)
            {
                await AppendKeyword(keywords[index], range, targetrange, navigating, readingView);
                if ((keywords[index].TrailingSymbols.IndexOf("<br>") > Result.notfound)
                    && (poetic))
                {
                    await writer.Append(HTMLTags.EndParagraph);
                    if (index < keywords.Count - 1)
                    {
                        await writer.Append(HTMLTags.StartParagraphWithClass);
                        await writer.Append(
                            (keywords[index + 1].WordIndex == Ordinals.first) ?
                            HTMLClasses.poetic1 :
                            HTMLClasses.poetic2);
                        await writer.Append(HTMLTags.CloseQuoteEndTag);
                    }
                }
            }
            if (range.Contains(keywords[Ordinals.last].ID) && (range.EndID.ID > keywords[Ordinals.last].ID))
            {
                await writer.Append(HTMLTags.EndMark);
            }
        }

        private async Task AppendKeyword(Keyword keyword, CitationRange range, CitationRange targetrange, bool navigating, bool readingView)
        {
            if (keyword.WordIndex == Ordinals.first)
            {
                KeyID lastID = new KeyID(keyword.Book, keyword.Chapter, 
                    keyword.Verse - 1, KeyID.MaxWordIndex);
                if (!navigating && (targetrange.EndID.ID == lastID.ID))
                {
                    await writer.Append(HTMLTags.EndMark);
                }
                await AppendVerseNumber(keyword, range, readingView);
            }
            if (keyword.IsPoetic != poetic)
            {
                if (poetic)
                {
                    poetic = false;
                    await writer.Append(HTMLTags.EndParagraph);
                }
                else
                {
                    poetic = true;
                    await writer.Append(HTMLTags.StartParagraphWithClass);
                    await writer.Append(HTMLClasses.poetic1);
                    await writer.Append(HTMLTags.CloseQuoteEndTag);
                }
            }
            if (!navigating && readingView && (targetrange.StartID.ID == keyword.ID))
            {
                await writer.Append(HTMLTags.StartSpanWithClass +
                    HTMLClasses.target +
                    HTMLTags.CloseQuoteEndTag +
                    HTMLTags.EndSpan);
            }
            if (!navigating && (keyword.ID == targetrange.StartID.ID))
            {
                await writer.Append(HTMLTags.StartMark);
            }
            await AppendTextOfKeyword(writer, keyword);
            if (!navigating && (keyword.ID == targetrange.EndID.ID))
            {
                await writer.Append(HTMLTags.EndMark);
            }
        }

        private async Task<bool> AppendFirstWord(List<Keyword> keywords, CitationRange range, CitationRange targetrange, bool navigating, bool readingView)
        {
            poetic = keywords[Ordinals.first].IsPoetic;
            if (poetic)
            {
                await writer.Append(HTMLTags.StartParagraphWithClass);
                await writer.Append(HTMLClasses.poetic1);
                await writer.Append(HTMLTags.CloseQuoteEndTag);
            }
            else
            {
                await writer.Append(HTMLTags.StartParagraph);
            }
            if (!navigating && readingView && (targetrange.StartID.ID == keywords[Ordinals.first].ID))
            {
                await writer.Append(HTMLTags.StartSpanWithClass +
                    HTMLClasses.target +
                    HTMLTags.CloseQuoteEndTag +
                    HTMLTags.EndSpan);
            }
            if (!navigating && (targetrange.Contains(keywords[Ordinals.first].ID)))
            {
                await writer.Append(HTMLTags.StartMark);
            }
            await AppendVerseNumber(keywords[Ordinals.first], range, readingView);
            if (keywords[Ordinals.first].WordIndex != Ordinals.first)
            {
                await writer.Append(Symbol.ellipsis);
            }
            await AppendTextOfKeyword(writer, keywords[Ordinals.first]);
            return poetic;
        }

        public async static Task AppendTextOfKeyword(HTMLWriter writer, Keyword keyword)
        {
            await writer.Append(keyword.LeadingSymbols.ToString());
            if (keyword.IsCapitalized)
            {
                await writer.Append(keyword.Text.Slice(Ordinals.first, 1).ToString().ToUpperInvariant());
                await writer.Append(keyword.Text.Slice(Ordinals.second).ToString());
            }
            else
            {
                await writer.Append(keyword.Text.ToString().Replace('`', '’'));
            }
            await writer.Append(keyword.TrailingSymbols.ToString());
        }

        private async Task AppendVerseNumber(Keyword keyword, CitationRange range, bool readingView)
        {
            await writer.Append(HTMLTags.StartSpanWithClass +
                HTMLClasses.versenumber);
            if (keyword.Verse == 1)
            {
                await writer.Append(Symbol.space +
                HTMLClasses.dropcap +
                HTMLTags.CloseQuoteEndTag);
            }
            else
            if (readingView)
            {
                await writer.Append(Symbol.space + HTMLClasses.readingVerse
                    + HTMLTags.CloseQuoteEndTag);
            }
            else
            {
                await writer.Append(HTMLTags.CloseQuoteEndTag);
            }
            var citation = new Citation(keyword.ID, Math.Min(keyword.ID + KeyID.MaxWordIndex, range.EndID.ID));
            if (readingView)
                citation.CitationType = CitationTypes.Text;
            else
                citation.CitationType = CitationTypes.Verse;


            await writer.Append(HTMLTags.StartBold);
            await PageFormatter.StartCitationLink(writer, citation);
            if (keyword.Verse == 1)
            {
                await writer.Append(keyword.Chapter);
            }
            else
            {
                await writer.Append(keyword.Verse);
            }
            await writer.Append(HTMLTags.EndAnchor +
                HTMLTags.EndBold+
                HTMLTags.NonbreakingSpace+
                HTMLTags.EndSpan);
            if (keyword.Verse == 1)
            {
                await writer.Append(HTMLTags.EndSpan);
            }
        }

        internal async Task AppendCitationData(Citation citation)
        {
            await writer.Append(HTMLTags.StartDivWithClass);
            await writer.Append(HTMLClasses.hidden + " " + HTMLClasses.active + " "+ HTMLClasses.rangeData);
            await writer.Append(HTMLTags.CloseQuote);
            await writer.Append(HTMLClasses.dataStart);
            await writer.Append(citation.CitationRange.StartID.ID);
            await writer.Append(HTMLClasses.dataEnd);
            await writer.Append(citation.CitationRange.EndID.ID);
            await writer.Append(HTMLTags.EndTag);
            await writer.Append(HTMLTags.EndDiv);
        }
    }
}