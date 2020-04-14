using System.Threading.Tasks;
using System.Collections.Generic;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.Data;
using Feliciana.ResponseWriter;
using Myriad.Library;
using Myriad.Pages;
using Myriad.Data;
using Myriad.Parser;

namespace Myriad.Search
{
    internal class SearchFormatter
    {
        private async Task AppendSearchResults(HTMLWriter writer, List<SearchSentence> results)
        {
            await writer.Append(HTMLTags.StartDivWithID);
            await writer.Append("results");
            await writer.Append(HTMLTags.CloseQuote +
                HTMLTags.Class);
            await writer.Append("searchResults");
            await writer.Append(HTMLTags.CloseQuoteEndTag);
            if (results.Count > Number.nothing)
            {
                await AppendSearchResults(0, 100, writer, results);
            }
            else
                await AppendNoResults(writer);
        }


        public static async Task AppendSearchResults(int startIndex, int endIndex, HTMLWriter writer, List<SearchSentence> searchResults)
        {
            if ((searchResults == null) ||
                ((searchResults.Count == 1) && (searchResults[0].SentenceID == Result.notfound)))
            {
                await AppendNoResults(writer);
                return;
            }
            int index = startIndex;
            bool hasResults = false;
            while ((index <= endIndex) && (index < searchResults.Count))
            {
                await writer.Append(HTMLTags.StartParagraph);
                await FormatSearchResult(writer, searchResults[index]);
                await writer.Append(HTMLTags.EndParagraph);
                hasResults = true;
                index++;
            }
            if (!hasResults) await AppendNoResults(writer);
        }

        internal static async Task AppendNoResults(HTMLWriter writer)
        {
            await writer.Append("<p>No results.</p>");
        }

        private static async Task FormatSearchResult(HTMLWriter writer, SearchSentence searchSentence)
        {
            int startID = -1;
            int endID = -1;
            int sentenceID = searchSentence.SentenceID;
            if (sentenceID == Result.notfound) return;
            var reader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadKeywordSentence),
                sentenceID);
            List<Keyword> sentenceKeywords = await reader.GetClassData<Keyword>();
            reader.Close();
            List<SearchResultWord> searchresultwords = new List<SearchResultWord>();
            for (int index = Ordinals.first; index < sentenceKeywords.Count; index++)
            {
                searchresultwords.Add(new SearchResultWord());
            }
            if (searchresultwords.Count == Number.nothing) return;
            var links = new Dictionary<int, (int, int)>();
            List<int> endLinks = new List<int>();
            foreach (SearchResult word in searchSentence.Words)
            {
                int start = word.WordIndex - 4;
                if (start < 0) start = Ordinals.first;
                if (startID == -1) startID = searchresultwords[start].ID;
                int end = word.WordIndex + word.Length + 3;
                if (end >= searchresultwords.Count) end = searchresultwords.Count - 1;
                endID = searchresultwords[end].ID;
                int highlight = word.WordIndex + word.Length - 1;
                if (highlight >= searchresultwords.Count) highlight = searchresultwords.Count - 1;
                if (word.ArticleID != -1)
                {
                    if (links.ContainsKey(word.WordIndex))
                    {
                        if (highlight > links[word.WordIndex].Item2)
                        {
                            endLinks.Remove(links[word.WordIndex].Item2);
                            links[word.WordIndex] = (word.ArticleID, highlight);
                            endLinks.Add(highlight);
                        }
                    }
                    else
                    {
                        links.Add(word.WordIndex, (word.ArticleID, highlight));
                        endLinks.Add(highlight);
                    }
                }
                if (word.Substitute)
                {
                    //if (word.Length > 1) await FormatPhraseText(writer, word);
                    highlight = word.WordIndex;
                    if (searchresultwords[word.WordIndex].IsMainText)
                        searchresultwords[word.WordIndex].SubstituteText = word.Text;
                    else
                        searchresultwords[word.WordIndex].SubstituteText = word.Text.Replace('[', '(').Replace(']', ')');
                    if (word.Length > 1)
                    {
                        int i = word.WordIndex + 1;
                        string trailing = "";
                        while ((i < word.WordIndex + word.Length) && (i < searchresultwords.Count))
                        {
                            searchresultwords[i].Erased = true;
                            i++;
                        }
                    }
                }
                int index = start;
                while (index < word.WordIndex)
                {
                    if (word.WordIndex >= searchresultwords.Count) break;
                    searchresultwords[index].Used = true;
                    index++;
                }
                while (index <= highlight)
                {
                    searchresultwords[index].Highlight = true;

                    searchresultwords[index].Used = true;
                    index++;
                }
                while (index <= end)
                {
                    searchresultwords[index].Used = true;
                    index++;
                }
            }
            Citation citation = new Citation(startID, endID);
            await CitationConverter.AppendLink(writer, citation);
            await writer.Append(": ");
            bool ellipsis = false;

            for (int idx = Ordinals.first; idx<searchresultwords.Count; idx++)
            {
                idx++;
                if (searchresultwords[idx].Erased)
                {
                    if (endLinks.Contains(idx)) await writer.Append(HTMLTags.EndAnchor);
                    continue;
                }
                if (!searchresultwords[idx].Used)
                {
                    if (!ellipsis)
                    {
                        await writer.Append("&hellip;");
                        if (idx != Ordinals.first) await writer.Append(" ");
                        ellipsis = true;
                    }
                    continue;
                }

                ellipsis = false;
                if (links.ContainsKey(idx))
                {
                    await writer.Append("<a HREF=" + ArticlePage.pageURL + "id=");
                    await writer.Append(links[idx].Item1);
                    await writer.Append("&tgstart=");
                    await writer.Append(startID);
                    await writer.Append("&tgend=");
                    await writer.Append(endID);
                    await writer.Append(">");

                }
                await AppendSearchResultWord(writer, searchresultwords[idx], sentenceKeywords[idx]);
                if (endLinks.Contains(idx)) await writer.Append("</a>");
            }
        }


        public static async Task AppendSearchResultWord(HTMLWriter writer, SearchResultWord searchResultWord, Keyword keyword)
        {
            if (searchResultWord.Erased) return;
            if (searchResultWord.Substituted)
            {
                if (keyword.IsCapitalized)
                {
                    await writer.Append("<b>");
                    await writer.Append(keyword.LeadingSymbolString);
                    if (searchResultWord.IsMainText)
                        await writer.Append("[");
                    else
                        await writer.Append("(");
                    await writer.Append(Symbols.Capitalize(searchResultWord.SubstituteText).Replace('_', ' '));
                    if (searchResultWord.SubstituteText.IndexOf(' ') == Result.notfound)
                    {
                        if (searchResultWord.IsMainText)
                            await writer.Append("]");
                        else
                            await writer.Append(")");
                    }
                    await writer.Append(keyword.TrailingSymbolString);
                    await writer.Append("</b> ");
                }
                else
                {
                    await writer.Append("<b>");
                    await writer.Append(keyword.LeadingSymbolString);
                    if (searchResultWord.IsMainText)
                        await writer.Append("[");
                    else
                        await writer.Append("(");
                    await writer.Append(searchResultWord.SubstituteText.Replace('_', ' '));
                    if (searchResultWord.SubstituteText.IndexOf(' ') == Result.notfound)
                    {
                        if (searchResultWord.IsMainText)
                            await writer.Append("]");
                        else
                            await writer.Append(")");
                    }
                    await writer.Append(keyword.TrailingSymbolString);
                    await writer.Append("</b> ");
                }
                return;
            }
            if (searchResultWord.Highlight)
            {
                if (keyword.IsCapitalized)
                {
                    await writer.Append("<b>");
                    await writer.Append(keyword.LeadingSymbolString);
                    await writer.Append(Symbols.Capitalize(keyword.TextString).Replace('`', '’'));
                    await writer.Append(keyword.TrailingSymbolString);
                    await writer.Append("</b> ");
                }
                else
                {
                    await writer.Append("<b>");
                    await writer.Append(keyword.LeadingSymbolString);
                    await writer.Append(keyword.TextString.Replace('`', '’'));
                    await writer.Append(keyword.TrailingSymbolString);
                    await writer.Append("</b> ");
                }
                return;
            }
            if (searchResultWord.Used)
            {
                if (keyword.IsCapitalized)
                {
                    await writer.Append(keyword.LeadingSymbolString);
                    await writer.Append(Symbols.Capitalize(keyword.TextString).Replace('`', '’'));
                    await writer.Append(keyword.TrailingSymbolString);
                    await writer.Append(" ");
                }
                else
                {
                    await writer.Append(keyword.LeadingSymbolString);
                    await writer.Append(keyword.TextString.Replace('`', '’'));
                    await writer.Append(keyword.TrailingSymbolString);
                    await writer.Append(" ");
                }
            }
        }

    }
}