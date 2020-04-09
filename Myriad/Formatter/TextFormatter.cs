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

        public async Task AppendKeywords(List<Keyword> keywords, bool readingView)
        {
            await AppendFirstWord(keywords, readingView);
            for (int index = Ordinals.second; index < keywords.Count; index++)
            {
                await AppendKeyword(keywords[index], readingView);
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
        }

        private async Task AppendKeyword(Keyword keyword, bool readingView)
        {
            if (keyword.WordIndex == Ordinals.first)
            {
                await AppendVerseNumber(keyword, readingView);
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
            await AppendTextOfKeyword(keyword);
        }

        private async Task<bool> AppendFirstWord(List<Keyword> keywords, bool readingView)
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
            await AppendVerseNumber(keywords[Ordinals.first], readingView);
            if (keywords[Ordinals.first].WordIndex != Ordinals.first)
            {
                await writer.Append(Symbol.ellipsis);
            }
            await AppendTextOfKeyword(keywords[Ordinals.first]);
            return poetic;
        }

        private async Task AppendTextOfKeyword(Keyword keyword)
        {
            await writer.Append(keyword.LeadingSymbols.ToString());
            if (keyword.IsCapitalized)
            {
                await writer.Append(keyword.Text.Slice(Ordinals.first, 1).ToString().ToUpperInvariant());
                await writer.Append(keyword.Text.Slice(Ordinals.second).ToString());
            }
            else
            {
                await writer.Append(keyword.Text.ToString());
            }
            await writer.Append(keyword.TrailingSymbols.ToString());
        }

        private async Task AppendVerseNumber(Keyword keyword, bool readingView)
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
            var citation = new Citation(keyword.ID, keyword.ID);
            if (readingView)
                citation.CitationType = CitationTypes.Text;
            else
                citation.CitationType = CitationTypes.Verse;


            await writer.Append(HTMLTags.StartBold);
            await PageFormatter.StartCitationAnchor(writer, citation);
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
            await writer.Append(citation.CitationRange.StartID);
            await writer.Append(HTMLClasses.dataEnd);
            await writer.Append(citation.CitationRange.EndID);
            await writer.Append(HTMLTags.EndTag);
            await writer.Append(HTMLTags.EndDiv);
        }
    }
}