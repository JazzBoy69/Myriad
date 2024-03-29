﻿using System;
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
        int paragraphIndex;
        public TextFormatter(HTMLWriter writer)
        {
            this.writer = writer;
        }
        public async Task AppendCommentSpanKeywords(List<Keyword> keywords, Citation citation, int spanIndex, bool multi)
        {
            if (spanIndex == Ordinals.first) await StartParagraph(keywords);
            paragraphIndex = Ordinals.first;

            for (int index = Ordinals.first; index < keywords.Count; index++)
            {
                await StartPoetic(keywords[index]);
                await AddSpanVerseNumber(keywords, index, multi);
                await AppendTextOfReadingViewKeyword(writer, keywords[index], paragraphIndex);
                paragraphIndex++;
                await EndPoetic(keywords, index);
            }
        }
        internal async Task AppendCommentSpanKeywords(List<Keyword> keywords, Citation citation, Citation targetCitation, int spanIndex, bool multi)
        {
            if (spanIndex == Ordinals.first) {
                await StartParagraph(keywords);
            }
            paragraphIndex = Ordinals.first;
            for (int index = Ordinals.first; index < keywords.Count; index++)
            {
                await StartPoetic(keywords[index]);
                await AddSpanVerseNumber(keywords, index, multi);
                await StartReadingViewHighlighting(keywords[index], citation, targetCitation);
                await AppendTextOfReadingViewKeyword(writer, keywords[index], paragraphIndex);
                paragraphIndex++;
                await EndHighlight(keywords[index], targetCitation);
                await EndPoetic(keywords, index, targetCitation);
            }
            await EndSectionHighlight(keywords[Ordinals.last], targetCitation);
        }
        public async Task AppendReadingViewKeywords(List<Keyword> keywords, Citation citation)
        {
            await StartParagraph(keywords);
            paragraphIndex = Ordinals.first;
            for (int index = Ordinals.first; index < keywords.Count; index++)
            {
                if (index > Ordinals.first) await StartNewParagraph(keywords[index]);
                await AddVerseNumber(keywords, index, citation);
                await AppendTextOfReadingViewKeyword(writer, keywords[index], paragraphIndex);
                paragraphIndex++;
                await EndPoetic(keywords, index);
            }
        }
        internal async Task AppendReadingViewKeywords(List<Keyword> keywords, Citation citation, Citation targetCitation)
        {
            if (targetCitation == null)
            {
                await AppendReadingViewKeywords(keywords, citation);
                return;
            }
            targetCitation = await CitationConverter.ResolveCitation(targetCitation);
            citation = await CitationConverter.ResolveCitation(citation);
            await StartParagraph(keywords);
            paragraphIndex = Ordinals.first;
            for (int index = Ordinals.first; index < keywords.Count; index++)
            {
                if (index > Ordinals.first) await StartNewParagraph(keywords[index]); 
                await AddVerseNumber(keywords, index, citation);
                await StartReadingViewHighlighting(keywords[index], citation, targetCitation);
                await AppendTextOfReadingViewKeyword(writer, keywords[index], paragraphIndex);
                paragraphIndex++;
                await EndHighlight(keywords[index], targetCitation);
                await EndPoetic(keywords, index, targetCitation);
            }
            await EndSectionHighlight(keywords[Ordinals.last], targetCitation);
        }
        internal async Task StartNewParagraph(Keyword keyword)
        {
            if (poetic != keyword.IsPoetic)
            {
                paragraphIndex = Ordinals.first;
                await TogglePoetic(keyword);
            }
            if (keyword.ParagraphWordIndex == Ordinals.first)
            {
                paragraphIndex = Ordinals.first;
                await writer.Append(HTMLTags.EndParagraph +
                    HTMLClasses.StartVerseParagraph);
            }
        }

        internal async Task StartPoetic(Keyword keyword)
        {
            if (poetic != keyword.IsPoetic)
            {
                paragraphIndex = Ordinals.first;
                await TogglePoetic(keyword);
            }
        }
        private async Task AddVerseNumber(List<Keyword> keywords, int index, Citation citation)
        {
            if ((keywords[index].WordIndex == Ordinals.first) || (index == Ordinals.first))
                await AppendReadingViewVerseNumber(keywords[index], citation);
        }

        private async Task AddSpanVerseNumber(List<Keyword> keywords, int index, bool multi)
        {
            if (keywords[index].WordIndex == Ordinals.first) 
                await AppendSpanVerseNumber(keywords[index], multi);
        }

        private async Task EndPoetic(List<Keyword> keywords, int index, Citation targetCitation)
        {
            if (poetic && (keywords[index].TrailingSymbols.IndexOf("<br>") > Result.notfound))
            {
                paragraphIndex = Ordinals.first;
                await StartNewPoeticParagraph(keywords, index, targetCitation);
            }
        }

        private async Task EndPoetic(List<Keyword> keywords, int index)
        {
            if (poetic && keywords[index].PoeticBreak)
            {
                paragraphIndex = Ordinals.first;
                await StartNewPoeticParagraph(keywords, index);
            }
        }

        private async Task TogglePoetic(Keyword keyword)
        {
            poetic = keyword.IsPoetic;
            if (poetic)
            {
                await StartPoeticParagraph(keyword);
            }
            else
            {
                await writer.Append(HTMLTags.EndParagraph +
                    HTMLClasses.StartVerseParagraph);
            }
        }

        private async Task EndSectionHighlight(Keyword lastKeyword, Citation targetCitation)
        {
            if (targetCitation.Contains(lastKeyword.ID) &&
                (targetCitation.End > lastKeyword.ID))
            {
                await writer.Append(HTMLTags.EndMark);
            }
        }

        private async Task EndHighlight(Keyword keyword, Citation targetCitation)
        {
            if (keyword.ID == targetCitation.End)
            {
                await writer.Append(HTMLTags.EndMark);
            }
        }

        private async Task StartReadingViewHighlighting(Keyword keyword, Citation citation, Citation targetCitation)
        {
            if ((targetCitation.Start == keyword.ID) ||
                ((paragraphIndex == Ordinals.first) &&
                (targetCitation.Start < keyword.ID) &&
                (targetCitation.End >= keyword.ID)))
            {
                await AppendReadingViewHighlightFormatting();
            }
        }
        private async Task StartReadingViewHighlighting(Citation citation, Citation targetCitation)
        {
            if ((targetCitation.Start < citation.Start) &&
                (targetCitation.End >= citation.Start))
            {
                await AppendReadingViewHighlightFormatting();
            }
        }

        internal async Task AppendKeywords(List<Keyword> keywords, Citation citation)
        {
            await StartParagraph(keywords);
            for (int index = Ordinals.first; index < keywords.Count; index++)
            {
                if (index > Ordinals.first) await StartNewParagraph(keywords[index]);
                await AppendVerseNumber(keywords, index, citation);
                await AppendLeadingSymbols(writer, keywords[index]);
                await AppendTextOfKeyword(writer, keywords[index]);
                await EndPoetic(keywords, index);
            }
        }
        internal async Task AppendKeywords(List<Keyword> keywords, Citation citation, Citation targetCitation)
        {
            targetCitation = await CitationConverter.ResolveCitation(targetCitation);
            citation = await CitationConverter.ResolveCitation(citation);
            if (citation.Equals(targetCitation) || !targetCitation.Valid)
            {
                await AppendKeywords(keywords, citation);
                return;
            }
            await StartParagraph(keywords);
            paragraphIndex = Ordinals.first;
            for (int index = Ordinals.first; index < keywords.Count; index++)
            {
                if (index > Ordinals.first) await StartNewParagraph(keywords[index]);
                await AppendVerseNumber(keywords, index, citation);
                await AppendLeadingSymbols(writer, keywords[index]);
                await StartHighlighting(keywords[index], citation, targetCitation);
                await AppendTextOfKeyword(writer, keywords[index]);
                paragraphIndex++;
                await EndHighlight(keywords[index], targetCitation);
                await EndPoetic(keywords, index, targetCitation);

            }
            await EndSectionHighlight(keywords[Ordinals.last], targetCitation);
        }

        private async Task StartHighlighting(Keyword keyword, Citation citation, Citation targetCitation)
        {
            if ((targetCitation.Start == keyword.ID) ||
                ((paragraphIndex == Ordinals.first) &&
                (targetCitation.Start < keyword.ID) &&
                (targetCitation.End > keyword.ID)))
            {
                await AppendHighlightFormatting();
            }
        }

        private async Task AppendHighlightFormatting()
        {
                await writer.Append(
                    HTMLTags.StartMark);
        }

        private async Task StartParagraph(List<Keyword> keywords)
        {
            poetic = keywords[Ordinals.first].IsPoetic;
            if (poetic)
            {
                await writer.Append(HTMLTags.StartParagraphWithClass);
                await writer.Append(HTMLClasses.poetic1);
                await writer.Append(HTMLTags.CloseQuoteEndTag);
                return;
            }
            if (keywords[Ordinals.first].Verse == 1)
            {
                await writer.Append(HTMLClasses.StartFirstVerseParagraph);
                return;
            }
            await writer.Append(HTMLClasses.StartVerseParagraph);
        }

        private async Task StartNewPoeticParagraph(List<Keyword> keywords, int index)
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
            paragraphIndex = Ordinals.first;
            poetic = true;
        }

        private async Task StartNewPoeticParagraph(List<Keyword> keywords, int index, Citation targetCitation)
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
                if (targetCitation.Contains(keywords[index + 1].ID))
                {
                    await writer.Append(HTMLTags.StartMark);
                }
            }
            paragraphIndex = Ordinals.first;
            poetic = true;
        }

        private async Task StartPoeticParagraph(Keyword keyword)
        {
            await writer.Append(HTMLTags.StartParagraphWithClass);
            await writer.Append(
                (keyword.WordIndex == Ordinals.first) ?
                HTMLClasses.poetic1 :
                HTMLClasses.poetic2);
            await writer.Append(HTMLTags.CloseQuoteEndTag);
            poetic = true;
        }


        private async Task AppendReadingViewHighlightFormatting()
        {
            await writer.Append(HTMLTags.StartSpanWithClass +
                HTMLClasses.target +
                HTMLTags.CloseQuoteEndTag +
                HTMLTags.EndSpan +
                HTMLTags.StartMark);
        }

        public static async Task AppendTextOfReadingViewKeyword(HTMLWriter writer, Keyword keyword, int paragraphIndex)
        {
            if (keyword.StartFootnote) 
            {
                await writer.Append(HTMLClasses.startExtraInfo);
            }
            await AppendReadingLeadingSymbols(writer, keyword, paragraphIndex);
            await AppendReadingText(writer, keyword);
            await AppendReadingTrailingSymbols(writer, keyword);
        }

        private static async Task AppendReadingTrailingSymbols(HTMLWriter writer, Keyword keyword)
        {
            if (keyword.EndFootnote)
            {
                await writer.Append(']'+ HTMLTags.EndSpan);
                await writer.Append(keyword.TrailingSymbols[Ordinals.second..]);
            }
            else
            {
                await writer.Append(keyword.TrailingSymbols);
            }
        }

        private static async Task AppendReadingText(HTMLWriter writer, Keyword keyword)
        {
            string text = keyword.CapitalizedText;
            text = HideDiacritics(text);
            await writer.Append(text);
        }

        private async static Task AppendReadingLeadingSymbols(HTMLWriter writer, Keyword keyword, int paragraphIndex)
        {
            if ((keyword.WordIndex == Ordinals.first) && (paragraphIndex != Ordinals.first))
            {
                await writer.Append(HTMLTags.StartSpanWithClass+HTMLClasses.spacer+HTMLTags.CloseQuoteEndTag+
                    HTMLTags.EndSpan); 
                await writer.Append(keyword.LeadingSymbols.Substring(Ordinals.second));
            }
            else
            {
                await writer.Append(keyword.LeadingSymbols);
            }
        }

        private static string HideDiacritics(string text)
        {
            text = text.Replace("΄", HTMLClasses.startExtraInfo + "΄" + HTMLTags.EndSpan).Replace("·", HTMLClasses.startExtraInfo + "·" + HTMLTags.EndSpan);
            return text;
        }

        public async static Task AppendTextOfKeyword(HTMLWriter writer, Keyword keyword)
        {    
            await AppendText(writer, keyword);
            await writer.Append(keyword.TrailingSymbols.ToString());
        }

        private static async Task AppendText(HTMLWriter writer, Keyword keyword)
        {
            string text = keyword.CapitalizedText.Replace('`', '’');
            await writer.Append(text);
        }

        internal static async Task AppendLeadingSymbols(HTMLWriter writer, Keyword keyword)
        {
            if (keyword.WordIndex == Ordinals.first)
            {
                await writer.Append(keyword.LeadingSymbols.Substring(Ordinals.second));
            }
            else
            {
                await writer.Append(keyword.LeadingSymbols);
            }
        }

        public async static Task AppendCleanTextOfKeyword(HTMLWriter writer, Keyword keyword, bool hideFootnotes, bool hideDiacritics)
        {
            if (hideFootnotes && !keyword.IsMainText) return;
            await writer.Append(keyword.LeadingSymbols);
            await AppendCleanText(writer, keyword, hideDiacritics);
            await writer.Append(keyword.TrailingSymbols.ToString());
        }

        private static async Task AppendCleanText(HTMLWriter writer, Keyword keyword, bool hideDiacritics)
        {
            string text = keyword.CapitalizedText.Replace('`', '’');
            if (hideDiacritics) text = text.Replace("΄", "").Replace("·", "");
            await writer.Append(text);
        }

        private async Task AppendReadingViewVerseNumber(Keyword keyword, Citation citation)
        {
            await StartReadingVerseSpan(keyword);
            var thisVerse = GetCurrentVerse(keyword, citation);
            thisVerse.CitationType = CitationTypes.Text;
            await PageFormatter.StartCitationLink(writer, thisVerse);
            await AppendReadingNumber(keyword);
            await EndVerseSpan(keyword);
        }

        private async Task AppendSpanVerseNumber(Keyword keyword, bool multi)
        {
            await StartReadingVerseSpan(keyword);
            var thisVerse = GetSpanVerse(keyword);
            thisVerse.CitationType = CitationTypes.Text;
            await PageFormatter.StartSpanCitationLink(writer, thisVerse, multi);
            await AppendReadingNumber(keyword);
            await EndVerseSpan(keyword);
        }

        private async Task AppendReadingNumber(Keyword keyword)
        {
            if (keyword.WordIndex > Ordinals.first)
            {
                await writer.Append(HTMLTags.Ellipsis);
                return;
            }
            if (keyword.Verse == 0)
            {
                await writer.Append("Sup");
                return;
            }
            if (keyword.Verse == 1)
            {
                await writer.Append(keyword.Chapter);
                return;
            }
            await writer.Append(Symbol.space);
            await writer.Append(keyword.Verse);
        }

        private async Task StartReadingVerseSpan(Keyword keyword)
        {
            await writer.Append(HTMLTags.StartSpanWithClass +
                HTMLClasses.versenumber);
            if ((keyword.Verse == 1) && (keyword.WordIndex == Ordinals.first))
            {
                await writer.Append(Symbol.space +
                HTMLClasses.dropcap +
                HTMLTags.CloseQuoteEndTag);
            }
            else

                await writer.Append(Symbol.space + HTMLClasses.readingVerse
                    + HTMLTags.CloseQuoteEndTag);
        }

        private async Task AppendVerseNumber(List<Keyword> keywords, int index, Citation citation)
        {
            if ((keywords[index].WordIndex != Ordinals.first) && (index != Ordinals.first)) return;
            Keyword keyword = keywords[index];
            await StartVerseSpan(keyword);
            var thisVerse = GetCurrentVerse(keyword, citation);
            thisVerse.CitationType = CitationTypes.Verse;
            await PageFormatter.StartCitationLink(writer, thisVerse);
            await AppendVerseNumber(keyword);
            await EndVerseSpan(keyword);
        }

        private async Task EndVerseSpan(Keyword keyword)
        {
            await writer.Append(HTMLTags.EndAnchor +
                HTMLTags.EndSpan);
            if (keyword.Verse == 1)
            {
                await writer.Append(HTMLTags.EndSpan);
            }
        }

        private async Task AppendVerseNumber(Keyword keyword)
        {
            if (keyword.WordIndex != Ordinals.first)
            {
                await writer.Append(HTMLTags.Ellipsis);
                return;
            }
            if (keyword.Verse == 0)
            {
                await writer.Append("Sup");
                await writer.Append(HTMLTags.NonbreakingSpace);
                return;
            }
            if (keyword.Verse == 1)
            {
                await writer.Append(keyword.Chapter);
                return;
            }
            await writer.Append(Symbol.space);
            await writer.Append(keyword.Verse);
            await writer.Append(HTMLTags.NonbreakingSpace);
        }

        private Citation GetCurrentVerse(Keyword keyword, Citation citation)
        {
            Citation thisVerse = new Citation(keyword.Book, keyword.Chapter, keyword.Verse);
            if (citation.FirstVerse == thisVerse.FirstVerse)
            {
                if (citation.FirstWordIndex > Ordinals.first)
                {
                    thisVerse.SetFirstWordIndex(citation.FirstWordIndex);
                }
            }
            if (citation.LastVerse == thisVerse.LastVerse)
            {
                thisVerse.SetLastWordIndex(citation.LastWordIndex);
            }
            return thisVerse;
        }

        private Citation GetSpanVerse(Keyword keyword)
        {
            Citation thisVerse = new Citation(keyword.Book, keyword.Chapter, keyword.Verse);
            return thisVerse;
        }

        private async Task StartVerseSpan(Keyword keyword)
        {
            await writer.Append(HTMLTags.StartSpanWithClass +
            HTMLClasses.versenumber);
            if ((keyword.Verse == 1) && (keyword.WordIndex == Ordinals.first))
            {
                await writer.Append(Symbol.space +
                HTMLClasses.dropcap +
                HTMLTags.CloseQuoteEndTag);
            }
            else
            {
                await writer.Append(HTMLTags.CloseQuoteEndTag);
            }
        }

        internal async Task AppendCitationData(Citation citation)
        {
            await writer.Append(HTMLTags.StartDivWithClass);
            await writer.Append(HTMLClasses.hidden + " " + HTMLClasses.active + " "+ HTMLClasses.rangeData);
            await writer.Append(HTMLTags.CloseQuote);
            await writer.Append(HTMLClasses.dataStart);
            await writer.Append(citation.Start);
            await writer.Append(HTMLClasses.dataEnd);
            await writer.Append(citation.End);
            await writer.Append(HTMLTags.EndTag);
            await writer.Append(HTMLTags.EndDiv);
        }

        internal async Task AppendCleanQuote(List<Keyword> keywords)
        {
            for (int i = Ordinals.first; i < keywords.Count; i++)
            {
                await AppendCleanTextOfKeyword(writer, keywords[i], true, true);
            }
        }
    }
}