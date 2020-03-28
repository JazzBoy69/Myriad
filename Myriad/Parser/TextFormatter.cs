using Myriad.Data;
using Myriad.Parser;
using System;
using System.Collections.Generic;
using Myriad.Library;

namespace Myriad.Parser
{
    internal class TextFormatter
    {
        private HTMLResponse builder;
        private bool poetic;
        public TextFormatter(HTMLResponse builder)
        {
            this.builder = builder;
        }

        public void AppendKeywords(List<Keyword> keywords)
        {
            AppendFirstWord(keywords);
            for (int index = Ordinals.second; index < keywords.Count; index++)
            {
                AppendKeyword(keywords[index]);
                if ((keywords[index].TrailingSymbols.IndexOf("<br>") > Result.notfound)
                    && (poetic))
                {
                    builder.Append(HTMLTags.EndDiv);
                    if (index < keywords.Count - 1)
                    {
                        builder.StartDivWithClass(
                            (keywords[index + 1].WordIndex == Ordinals.first) ?
                            HTMLClasses.poetic1 :
                            HTMLClasses.poetic2);
                    }
                }
            }
        }

        private void AppendKeyword(Keyword keyword)
        {
            if (keyword.WordIndex == Ordinals.first)
            {
                AppendVerseNumber(keyword);
            }
            if (keyword.IsPoetic != poetic)
            {
                if (poetic)
                {
                    poetic = false;
                    builder.Append(HTMLTags.EndDiv);
                }
                else
                {
                    poetic = true;
                    builder.StartDivWithClass(HTMLClasses.poetic1);
                }
            }
            AppendTextOfKeyword(keyword);
        }

        private bool AppendFirstWord(List<Keyword> keywords)
        {
            AppendVerseNumber(keywords[Ordinals.first]);
            if (keywords[Ordinals.first].WordIndex != Ordinals.first)
            {
                builder.Append(Symbols.ellipsis);
            }
            poetic = keywords[Ordinals.first].IsPoetic;
            if (poetic)
            {
                builder.StartDivWithClass(HTMLClasses.poetic1);
            }
            AppendTextOfKeyword(keywords[Ordinals.first]);
            return poetic;
        }

        private void AppendTextOfKeyword(Keyword keyword)
        {
            builder.Append(keyword.LeadingSymbols);
            if (keyword.IsCapitalized)
            {
                Span<char> firstLetter = new Span<char>();
                keyword.Text.Slice(Ordinals.first, 1).ToUpper(firstLetter, 
                    System.Globalization.CultureInfo.CurrentCulture);
                builder.Append(firstLetter);
                builder.Append(keyword.Text.Slice(Ordinals.second));
            }
            else
            {
                builder.Append(keyword.Text);
            }
            builder.Append(keyword.TrailingSymbols);
        }

        private void AppendVerseNumber(Keyword keyword)
        {
            builder.Append(HTMLTags.StartAnchor);
            var citation = new Citation(keyword.ID, keyword.ID);
            citation.CitationType = CitationTypes.Verse;
            PageFormatter.StartCitationAnchor(builder, citation);
            builder.Append(keyword.Verse);
            builder.Append(HTMLTags.EndAnchor);
            builder.Append(Symbols.space);
        }

        internal void AppendCitationData(Citation citation)
        {
            builder.Append(HTMLTags.StartDivWithClass);
            builder.Append(HTMLClasses.hidden + " " + HTMLClasses.active + HTMLClasses.rangeData);
            builder.Append(" " + HTMLTags.dataStart+"='");
            builder.Append(citation.CitationRange.StartID);
            builder.Append("' " + HTMLTags.dataEnd + "='");
            builder.Append(citation.CitationRange.EndID);
            builder.Append(HTMLTags.CloseQuoteEndTag);
            builder.Append(HTMLTags.EndDiv);
        }
    }
}