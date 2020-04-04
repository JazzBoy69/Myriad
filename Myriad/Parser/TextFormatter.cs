using Myriad.Data;
using Myriad.Parser;
using System;
using System.Collections.Generic;
using Myriad.Library;

namespace Myriad.Parser
{
    internal class TextFormatter
    {
        private HTMLWriter builder;
        private bool poetic;
        public TextFormatter(HTMLWriter builder)
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
                    builder.Append(HTMLTags.EndParagraph);
                    if (index < keywords.Count - 1)
                    {
                        builder.StartParagraphWithClass(
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
                    builder.Append(HTMLTags.EndParagraph);
                }
                else
                {
                    poetic = true;
                    builder.StartParagraphWithClass(HTMLClasses.poetic1);
                }
            }
            AppendTextOfKeyword(keyword);
        }

        private bool AppendFirstWord(List<Keyword> keywords)
        {
            poetic = keywords[Ordinals.first].IsPoetic;
            if (poetic)
            {
                builder.StartParagraphWithClass(HTMLClasses.poetic1);
            }
            else
            {
                builder.Append(HTMLTags.StartParagraph);
            }
            AppendVerseNumber(keywords[Ordinals.first]);
            if (keywords[Ordinals.first].WordIndex != Ordinals.first)
            {
                builder.Append(Symbol.ellipsis);
            }
            AppendTextOfKeyword(keywords[Ordinals.first]);
            return poetic;
        }

        private void AppendTextOfKeyword(Keyword keyword)
        {
            builder.Append(keyword.LeadingSymbols);
            if (keyword.IsCapitalized)
            {
                builder.Append(keyword.Text.Slice(Ordinals.first, 1).ToString().ToUpperInvariant());
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
            var citation = new Citation(keyword.ID, keyword.ID);
            citation.CitationType = CitationTypes.Verse;
            builder.Append(HTMLTags.StartBold);
            PageFormatter.StartCitationAnchor(builder, citation);
            builder.Append(keyword.Verse);
            builder.Append(HTMLTags.EndAnchor);
            builder.Append(HTMLTags.EndBold);
            builder.Append(Symbol.space);
        }

        internal void AppendCitationData(Citation citation)
        {
            builder.Append(HTMLTags.StartDivWithClass);
            builder.Append(HTMLClasses.hidden + " " + HTMLClasses.active + " "+ HTMLClasses.rangeData);
            builder.Append(HTMLTags.CloseQuote);
            builder.Append(HTMLClasses.dataStart);
            builder.Append(citation.CitationRange.StartID);
            builder.Append(HTMLClasses.dataEnd);
            builder.Append(citation.CitationRange.EndID);
            builder.Append(HTMLTags.EndTag);
            builder.Append(HTMLTags.EndDiv);
        }
    }
}