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

        public async Task AppendReadingViewKeywords(List<Keyword> keywords, Citation citation)
        {
            await StartParagraph(keywords);
            int paragraphIndex = Ordinals.first;
            for (int index = Ordinals.first; index < keywords.Count; index++)
            {
                if (poetic != keywords[index].IsPoetic)
                {
                    paragraphIndex = Ordinals.first;
                    await TogglePoetic(keywords[index]);
                }
                if ((keywords[index].WordIndex == Ordinals.first) || (index==Ordinals.first))
                    await AppendReadingViewVerseNumber(keywords[index], citation);
                await AppendTextOfReadingViewKeyword(writer, keywords[index], paragraphIndex);
                paragraphIndex++;
                if (poetic)
                {
                    if (keywords[index].TrailingSymbols.IndexOf("<br>") > Result.notfound)
                    {
                        paragraphIndex = Ordinals.first;
                    }
                    await CheckForNewPoeticParagraph(keywords, index);
                }
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
                    HTMLTags.StartParagraph);
            }
        }

        internal async Task AppendReadingViewKeywords(List<Keyword> keywords, Citation citation, Citation targetCitation)
        {
            await StartParagraph(keywords);
            if ((targetCitation.CitationRange.StartID.ID < citation.CitationRange.StartID.ID) &&
                (targetCitation.CitationRange.EndID.ID >= citation.CitationRange.StartID.ID))
            {
                await AppendReadingViewHighlightFormatting();
            }
            int paragraphIndex = Ordinals.first;
            for (int index = Ordinals.first; index < keywords.Count; index++)
            {
                if (keywords[index].WordIndex == Ordinals.first)
                {
                    KeyID lastID = new KeyID(keywords[index].Book, keywords[index].Chapter,
                        keywords[index].Verse - 1, KeyID.MaxWordIndex);
                    if (targetCitation.CitationRange.EndID.ID == lastID.ID)
                    {
                        await writer.Append(HTMLTags.EndMark);
                    }
                    if (poetic != keywords[index].IsPoetic)
                    {
                        paragraphIndex = Ordinals.first;
                        await TogglePoetic(keywords[index]);
                    }
                    await AppendReadingViewVerseNumber(keywords[index], citation);
                }
                if ((targetCitation.CitationRange.StartID.ID == keywords[index].ID) || 
                    ((paragraphIndex==Ordinals.first) && 
                    (targetCitation.CitationRange.StartID.ID<keywords[index].ID) &&
                    (targetCitation.CitationRange.EndID.ID>keywords[index].ID)))
                {
                    await AppendReadingViewHighlightFormatting();
                }
                await AppendTextOfReadingViewKeyword(writer, keywords[index], paragraphIndex);
                if (targetCitation.CitationRange.EndID.ID == keywords[index].ID)
                {
                    await writer.Append(HTMLTags.EndMark);
                }
                if (poetic && (keywords[index].TrailingSymbols.IndexOf("<br>") > Result.notfound))
                {
                    paragraphIndex = Ordinals.first;
                    await StartNewPoeticParagraph(keywords, index, targetCitation);
                }
            }
            if (targetCitation.CitationRange.Contains(keywords[Ordinals.last].ID) &&
                (targetCitation.CitationRange.EndID.ID > keywords[Ordinals.last].ID))
            {
                await writer.Append(HTMLTags.EndMark);
            }
        }

        internal async Task AppendKeywords(List<Keyword> keywords, Citation citation)
        {
            await StartParagraph(keywords);
            for (int index = Ordinals.first; index < keywords.Count; index++)
            {
                if (poetic != keywords[index].IsPoetic)
                {
                    await TogglePoetic(keywords[index]);
                }
                if ((keywords[index].WordIndex==Ordinals.first) || (index == Ordinals.first))
                    await AppendVerseNumber(keywords[index], citation);
                await AppendTextOfKeyword(writer, keywords[index]);
                if (poetic) await CheckForNewPoeticParagraph(keywords, index);
            }
        }
        internal async Task AppendKeywords(List<Keyword> keywords, Citation citation, Citation targetCitation)
        {
            if (citation.Equals(targetCitation))
            {
                await AppendKeywords(keywords, citation);
                return;
            }
            await StartParagraph(keywords);
            int paragraphIndex = Ordinals.first;
            for (int index = Ordinals.first; index < keywords.Count; index++)
            {
                if (poetic != keywords[index].IsPoetic)
                {
                    await TogglePoetic(keywords[index]);
                    paragraphIndex = Ordinals.first;
                }
                if ((keywords[index].WordIndex == Ordinals.first) || (index==Ordinals.first))
                {
                    KeyID lastID = new KeyID(keywords[index].Book, keywords[index].Chapter,
                    keywords[index].Verse - 1, KeyID.MaxWordIndex);
                    if (targetCitation.CitationRange.EndID.ID == lastID.ID)
                    {
                        await writer.Append(HTMLTags.EndMark);
                    }
                    await AppendVerseNumber(keywords[index], citation);
                }
                if ((targetCitation.CitationRange.StartID.ID == keywords[index].ID) ||
                    ((paragraphIndex == Ordinals.first) &&
                    (targetCitation.CitationRange.StartID.ID < keywords[index].ID) &&
                    (targetCitation.CitationRange.EndID.ID > keywords[index].ID)))
                {
                    await writer.Append(HTMLTags.StartMark);
                }
                await AppendTextOfKeyword(writer, keywords[index]);
                paragraphIndex++;
                if (targetCitation.CitationRange.EndID.ID == keywords[index].ID)
                {
                    await writer.Append(HTMLTags.EndMark);
                }
                if (poetic && (keywords[index].TrailingSymbols.IndexOf("<br>") > Result.notfound))
                {
                    paragraphIndex = Ordinals.first;
                    await StartNewPoeticParagraph(keywords, index, targetCitation);
                }
            }
            if (targetCitation.CitationRange.Contains(keywords[Ordinals.last].ID) && 
                (targetCitation.CitationRange.EndID.ID > keywords[Ordinals.last].ID))
            {
                await writer.Append(HTMLTags.EndMark);
            }
        }

        private async Task StartParagraph(List<Keyword> keywords)
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
                if (targetCitation.CitationRange.Contains(keywords[index + 1].ID))
                {
                    await writer.Append(HTMLTags.StartMark);
                }
            }
            poetic = true;
        }
        private async Task CheckForNewPoeticParagraph(List<Keyword> keywords, int index)
        {
            if (keywords[index].TrailingSymbols.IndexOf("<br>") > Result.notfound)
            {
                await writer.Append(HTMLTags.EndParagraph);
                if (index < keywords.Count - 1)
                {
                    await StartPoeticParagraph(keywords[index + 1]);
                }
            }
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

        private async Task AppendPoeticFormatting(Keyword keyword, CitationRange targetrange)
        {
            if (keyword.IsPoetic != poetic)
            {
                await UpdatePoetic(keyword, targetrange);
                return;
            }
            if (keyword.ParagraphWordIndex == Ordinals.first)
            {
                await StartNewParagraph();
            }
        }

        private async Task StartNewParagraph()
        {
            if (poetic)
            {
                await writer.Append(HTMLTags.EndParagraph + HTMLTags.StartParagraphWithClass);
                await writer.Append(HTMLClasses.poetic1);
                await writer.Append(HTMLTags.CloseQuoteEndTag);
            }
            else
            {
                await writer.Append(HTMLTags.EndParagraph + HTMLTags.StartParagraph);
            }
        }

        private async Task UpdatePoetic(Keyword keyword, CitationRange targetrange)
        {
            if (poetic)
            {
                poetic = false;
                await writer.Append(HTMLTags.EndParagraph + HTMLTags.StartParagraph);
            }
            else
            {
                poetic = true;
                await writer.Append(HTMLTags.EndParagraph + HTMLTags.StartParagraphWithClass);
                await writer.Append(HTMLClasses.poetic1);
                await writer.Append(HTMLTags.CloseQuoteEndTag);
                if ((keyword.ID != targetrange.StartID.ID)
                    && (targetrange.Contains(keyword.ID)))
                {
                    await writer.Append(HTMLTags.StartMark);
                }
            }
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
            if (!keyword.IsMainText)
            {
                await writer.Append(HTMLClasses.startExtraInfo);
            }

            if ((keyword.WordIndex == Ordinals.first) && (paragraphIndex != Ordinals.first))
            {
                await writer.Append(HTMLTags.NonbreakingSpace);
                await writer.Append(keyword.LeadingSymbolString.Substring(Ordinals.second));
            }
            else
            {
                await writer.Append(keyword.LeadingSymbolsString);
            }

            if (keyword.IsCapitalized)
            {
                await writer.Append(keyword.Text.Slice(Ordinals.first, 1).ToString().ToUpperInvariant());
                string text = keyword.Text.Slice(Ordinals.second).ToString().Replace('`', '’');
                text = HideDiacritics(text);
                await writer.Append(text);
            }
            else
            {
                string text = keyword.Text.ToString().Replace('`', '’');
                text = HideDiacritics(text);
                await writer.Append(text);
            }
            string trailing = keyword.TrailingSymbolString;
            if ((trailing.Length > 0) && !keyword.IsMainText && (trailing[Ordinals.first] == ']'))
            {
                await writer.Append(trailing[Ordinals.first]);
                if (!keyword.IsMainText)
                {
                    await writer.Append(HTMLTags.EndSpan);
                }
                if (trailing.Length > 1) await writer.Append(trailing.Substring(Ordinals.second));
            }
            else
            {
                await writer.Append(keyword.TrailingSymbols.ToString());
                if (!keyword.IsMainText)
                {
                    await writer.Append(HTMLTags.EndSpan);
                }
            }
        }

        private static string HideDiacritics(string text)
        {
            text = text.Replace("΄", HTMLClasses.startExtraInfo + "΄" + HTMLTags.EndSpan).Replace("·", HTMLClasses.startExtraInfo + "·" + HTMLTags.EndSpan);
            return text;
        }

        public async static Task AppendTextOfKeyword(HTMLWriter writer, Keyword keyword)
        {
            if (keyword.WordIndex == Ordinals.first)
            {
                await writer.Append(keyword.LeadingSymbolsString.Substring(Ordinals.second));
            }
            else
            {
                await writer.Append(keyword.LeadingSymbolsString);
            }
            if (keyword.IsCapitalized)
            {
                await writer.Append(keyword.Text.Slice(Ordinals.first, 1).ToString().ToUpperInvariant());
                string text = keyword.Text.Slice(Ordinals.second).ToString().Replace('`', '’');       
                await writer.Append(text);
            }
            else
            {
                string text = keyword.Text.ToString().Replace('`', '’');    
                await writer.Append(text);
            }
            await writer.Append(keyword.TrailingSymbols.ToString());
        }
        public async static Task AppendCleanTextOfKeyword(HTMLWriter writer, Keyword keyword, bool hideFootnotes, bool hideDiacritics)
        {
            if (hideFootnotes && !keyword.IsMainText) return;
            await writer.Append(keyword.LeadingSymbolsString);
            if (keyword.IsCapitalized)
            {
                await writer.Append(keyword.Text.Slice(Ordinals.first, 1).ToString().ToUpperInvariant());
                string text = keyword.Text.Slice(Ordinals.second).ToString().Replace('`', '’');
                if (hideDiacritics) text = text.Replace("΄", "").Replace("·", "");
                await writer.Append(text);
            }
            else
            {
                string text = keyword.Text.ToString().Replace('`', '’');
                if (hideDiacritics) text = text.Replace("΄", "").Replace("·", "");
                await writer.Append(text);
            }
            await writer.Append(keyword.TrailingSymbols.ToString());
        }

        private async Task AppendReadingViewVerseNumber(Keyword keyword, Citation citation)
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

            var thisVerse = new Citation(keyword.Book, keyword.Chapter, keyword.Verse);
            if (citation.CitationRange.FirstVerse == thisVerse.CitationRange.FirstVerse)
            {
                if (citation.CitationRange.FirstWordIndex > Ordinals.first)
                {
                    thisVerse.CitationRange.SetFirstWordIndex(citation.CitationRange.FirstWordIndex);
                }
            }
            if (citation.CitationRange.LastVerse == thisVerse.CitationRange.LastVerse)
            {
                thisVerse.CitationRange.SetLastWordIndex(citation.CitationRange.LastWordIndex);
            }
                thisVerse.CitationType = CitationTypes.Text;

            await writer.Append(HTMLTags.StartBold);
            await PageFormatter.StartCitationLink(writer, thisVerse);
            if (keyword.WordIndex > Ordinals.first)
            {
                await writer.Append(HTMLTags.Ellipsis);
            }
            else
            if (keyword.Verse == 0)
            {
                await writer.Append("Sup");
            }
            else
            if (keyword.Verse == 1)
            {
                await writer.Append(keyword.Chapter);
            }
            else
            {
                await writer.Append(Symbol.space);
                await writer.Append(keyword.Verse);
            }
            await writer.Append(HTMLTags.EndAnchor +
                HTMLTags.EndBold +
                HTMLTags.EndSpan);
            if (keyword.Verse == 1)
            {
                await writer.Append(HTMLTags.EndSpan);
            }
        }

        private async Task AppendVerseNumber(Keyword keyword, Citation citation)
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
            var thisVerse = new Citation(keyword.Book, keyword.Chapter, keyword.Verse);
            if (citation.CitationRange.FirstVerse == thisVerse.CitationRange.FirstVerse)
            {
                if (citation.CitationRange.FirstWordIndex > Ordinals.first)
                {
                    thisVerse.CitationRange.SetFirstWordIndex(citation.CitationRange.FirstWordIndex);
                }
            }
            if (citation.CitationRange.LastVerse == thisVerse.CitationRange.LastVerse)
            {
                thisVerse.CitationRange.SetLastWordIndex(citation.CitationRange.LastWordIndex);
            }
                thisVerse.CitationType = CitationTypes.Verse;


            await writer.Append(HTMLTags.StartBold);
            await PageFormatter.StartCitationLink(writer, thisVerse);
            if (keyword.WordIndex != Ordinals.first)
            {
                await writer.Append(HTMLTags.Ellipsis);
            }
            else
            if (keyword.Verse == 0)
            {
                await writer.Append("Sup");
            }
            else
            if (keyword.Verse == 1)
            {
                await writer.Append(keyword.Chapter);
            }
            else
            {
                await writer.Append(Symbol.space);
                await writer.Append(keyword.Verse);
                await writer.Append(HTMLTags.NonbreakingSpace);
            }
            await writer.Append(HTMLTags.EndAnchor +
                HTMLTags.EndBold+
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

        internal async Task AppendCleanQuote(List<Keyword> keywords)
        {
            for (int i = Ordinals.first; i < keywords.Count; i++)
            {
                await AppendCleanTextOfKeyword(writer, keywords[i], true, true);
            }
        }
    }
}