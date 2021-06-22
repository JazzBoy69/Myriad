using Feliciana.ResponseWriter;
using Microsoft.AspNetCore.Http;
using Myriad.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.HTML;
using Myriad.Parser;
using Myriad.Data;
using Myriad.Formatter;

namespace Myriad.Pages
{
    public class Sidebar
    {
        internal static readonly string pageURL = "/Sidebar";
        public static async Task HandleRequest(HTMLWriter writer, IQueryCollection query)
        {
            Citation citation = GetCitation(query);
            citation.CitationType = CitationTypes.Text;
            await WriteHeader(writer, citation);
            //Add text of scriptures
            await WriteScriptureText(writer, citation);
        }

        private static async Task WriteScriptureText(HTMLWriter writer, Citation citation)
        {
            List<Keyword> keywords = TextSectionFormatter.ReadKeywords(citation);
            await writer.Append(HTMLTags.StartDivWithClass +
                HTMLClasses.sidebarScripture +
                HTMLTags.CloseQuoteEndTag);
            await AppendKeywords(writer, keywords, citation);
            await writer.Append(HTMLTags.EndDiv);
        }

        private static async Task AppendKeywords(HTMLWriter writer, List<Keyword> keywords, Citation citation)
        {
            bool poetic = false;
            for (int index = Ordinals.first; index < keywords.Count; index++)
            {
                await StartPoetic(writer, keywords[index], poetic);
                poetic = keywords[index].IsPoetic;
                if ((index == Ordinals.first) || (keywords[index].WordIndex == Ordinals.first))
                {
                    await AppendVerseNumber(writer, keywords[index]);
                }
                await TextFormatter.AppendLeadingSymbols(writer, keywords[index]);
                await TextFormatter.AppendTextOfKeyword(writer, keywords[index]);
                if (index < keywords.Count - 1)
                {
                    await EndPoetic(writer, keywords[index], poetic);
                }
                else
                {
                    if (poetic)
                    {
                        await writer.Append(HTMLTags.EndParagraph);
                    }
                }
            }
        }

        internal static async Task StartPoetic(HTMLWriter writer, Keyword keyword, bool poetic)
        {
            if (poetic != keyword.IsPoetic)
            {
                await TogglePoetic(writer, keyword);
            }
        }
        private static async Task TogglePoetic(HTMLWriter writer, Keyword keyword)
        {
            if (keyword.IsPoetic)
            {
                await StartPoeticParagraph(writer);
            }
            else
            {
                await writer.Append(HTMLTags.EndParagraph +
                    HTMLTags.StartParagraph);
            }
        }
        private static async Task AppendVerseNumber(HTMLWriter writer, Keyword keyword)
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
            await writer.Append(Symbol.space);
            await writer.Append(keyword.Verse);
            await writer.Append(HTMLTags.NonbreakingSpace);
        }
        private static async Task EndPoetic(HTMLWriter writer, Keyword keyword, bool poetic)
        {
            if (poetic && (keyword.TrailingSymbols.IndexOf("<br>") > Result.notfound))
            {
                await StartPoeticParagraph(writer);
            }
        }

        private static async Task StartPoeticParagraph(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.EndParagraph);
            await writer.Append(HTMLTags.StartParagraphWithClass);
            await writer.Append(HTMLClasses.poetic1);
            await writer.Append(HTMLTags.CloseQuoteEndTag);
        }
        private static async Task WriteHeader(HTMLWriter writer, Citation citation)
        {
            await writer.Append(HTMLTags.StartDivWithClass + HTMLClasses.closesidebar +
                HTMLTags.CloseQuote + HTMLTags.OnClick + "HideSidebar()" +
                HTMLTags.EndTag + "&times;" + HTMLTags.EndDiv);
            await writer.Append(HTMLTags.StartParagraph);
            await PageFormatter.StartCitationLink(writer, citation);
            await CitationConverter.Append(writer, citation);
            await writer.Append(HTMLTags.EndAnchor);
            await writer.Append(HTMLTags.EndParagraph);
        }

        private static Citation GetCitation(IQueryCollection query)
        {
            if (!int.TryParse(query[ScripturePage.queryKeyStart], out int start)) start = Result.notfound;
            if (!int.TryParse(query[ScripturePage.queryKeyEnd], out int end)) end = Result.notfound;
            Citation citation = new Citation(start, end);
            return citation;
        }

    }
}
