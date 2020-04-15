using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Feliciana.Library;
using Feliciana.HTML;
using Feliciana.Data;
using Feliciana.ResponseWriter;
using Myriad.Library;
using Myriad.Pages;
using Myriad.Data;
using Myriad.Parser;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;

namespace Myriad.Search
{
    internal class SearchFormatter
    {
        internal static async Task FormatBody(HTMLWriter writer, SearchPageInfo pageInfo)
        {
            await WriteInitialHTML(writer, pageInfo);

            await WriteDefinitionsBlock(writer, pageInfo);
            await writer.Append(HTMLTags.EndDiv);

            await AppendSearchResults(0, 100, writer, pageInfo.SearchResults);

            await writer.Append(HTMLTags.StartDivWithClass);
            await writer.Append("synresults");
            await writer.Append(HTMLTags.CloseQuoteEndTag +
                HTMLTags.EndDiv +
                HTMLTags.EndDiv);
        }

        private static async Task WriteInitialHTML(HTMLWriter writer, SearchPageInfo pageInfo)
        {
            await writer.Append("<a ID=querystring class=hidden href=");
            await writer.Append(pageInfo.Query.Replace(' ', '_'));
            await writer.Append(">");
            await writer.Append(HTMLTags.EndAnchor);
            await writer.Append(HTMLTags.StartDivWithID);
            await writer.Append("definitionDiv");
            await writer.Append(HTMLTags.CloseQuote +
                HTMLTags.Class);
            await writer.Append("searchTabs");
            await writer.Append(HTMLTags.CloseQuoteEndTag);
        }

        internal async static Task WriteDefinitionsBlock(HTMLWriter writer, SearchPageInfo pageInfo)
        {
            int mainDefinition = await IdentifyMainDefinition(pageInfo);
            await StartDefinitionsTitles(writer);
            bool active = false;
            int itemCount = Ordinals.first;
            Dictionary<string, int> headings = await GetDefinitionHeadings(pageInfo);
            await WriteDefinitionHeadings(writer, mainDefinition, headings, pageInfo);
            active = false;
            itemCount = Ordinals.first;
            MarkupParser parser = new MarkupParser(writer);
            foreach (KeyValuePair<string, int> entry in headings.OrderBy(e => e.Key))
            {
                int id = entry.Value;
                bool setActive = (mainDefinition == Result.notfound) || (entry.Value == mainDefinition);
                await StartDefinitionTab(writer, setActive, itemCount);
                string definition = await GetArticleParagraph(id, Ordinals.first);
                if (!string.IsNullOrEmpty(definition))
                {
                    await WriteDefinition(writer, parser, id, definition);
                    List<(int id, int index)> usedParagraphs = new List<(int, int)>() { (id, 0) };
                    foreach (int otherID in pageInfo.UsedDefinitions)
                    {
                        if (otherID == id) continue;
                        parser.SetParagraphInfo(ParagraphType.Article, otherID);
                        List<int> relatedParagraphIndices = await GetRelatedParagraphIndices(id, otherID);
                        foreach (int paragraphIndex in relatedParagraphIndices)
                        {
                            (int id, int index) key = (id, paragraphIndex);
                            if (usedParagraphs.Contains(key)) continue;
                            usedParagraphs.Add(key);
                            await WriteRelatedParagraph(writer, parser, otherID, paragraphIndex);
                        }
                    }
                }
                await writer.Append("<p class=\"definitionnav\">");
                if ((!string.IsNullOrEmpty(definition)) &&
                    (pageInfo.IDs != null) && (!pageInfo.IDs.Contains(id.ToString())))
                {
                    await writer.Append("<a HREF=" + SearchPage.pageURL + "?q=");
                    await writer.Append(pageInfo.Query.Replace(' ', '+'));
                    await writer.Append("&ids=");
                    if (!string.IsNullOrEmpty(pageInfo.IDs))
                    {
                        await writer.Append(pageInfo.IDs);
                        await writer.Append("+");
                    }
                    await writer.Append(id);
                    await writer.Append(">Search&nbsp;for&nbsp;this&nbsp;definition</a> ");
                }
                await writer.Append("</li>");
                itemCount++;
            }
            await writer.Append("</ul></div></section>");
        }

        private static async Task WriteRelatedParagraph(HTMLWriter writer, MarkupParser parser, int otherID, int paragraphIndex)
        {
            await writer.Append("<p class=\"definition\">");
            string paragraphText = await GetArticleParagraph(otherID, paragraphIndex);
            await parser.ParseParagraph(paragraphText, paragraphIndex);
            await writer.Append("</p>");
        }

        private static async Task<List<int>> GetRelatedParagraphIndices(int id, int otherID)
        {
            var relatedReader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(
                            DataOperation.ReadRelatedParagraphIndex), id, otherID);
            List<int> paragraphs = await relatedReader.GetData<int>();
            relatedReader.Close();
            return paragraphs;
        }

        private static async Task WriteDefinition(HTMLWriter writer, MarkupParser parser, int id, string definition)
        {
            await writer.Append("<p class=\"definition\">");
            parser.SetParagraphInfo(ParagraphType.Article, id);
            await parser.ParseParagraph(definition, Ordinals.first);
            await writer.Append("</p>");
        }

        private static async Task<string> GetArticleParagraph(int id, int index)
        {
            var reader = new DataReaderProvider<int, int>(
                    SqlServerInfo.GetCommand(DataOperation.ReadArticleParagraph),
                    id, index);

            string definition = await reader.GetDatum<string>();
            reader.Close();
            return definition;
        }

        private static async Task StartDefinitionTab(HTMLWriter writer, bool setActive, int itemCount)
        {
            await writer.Append("<li id=\"tabs0-");
            await writer.Append(itemCount);
            await writer.Append("-tab\" ");
            if (setActive)
            {
                 await writer.Append(" class=\"active\"");
            }
            await writer.Append(">");
        }

        private static async Task StartDefinitionsTitles(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartDivWithClass);
            await writer.Append("definitionsheader");
            await writer.Append(HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartList +
                HTMLTags.ID);
            await writer.Append("tabs0");
            await writer.Append(HTMLTags.CloseQuote +
                HTMLTags.Class);
            await writer.Append("tabs");
            await writer.Append(HTMLTags.CloseQuoteEndTag);
        }

        private static async Task<int> IdentifyMainDefinition(SearchPageInfo pageInfo)
        {
            int mainDefinition = Result.notfound;
            int lowestIndex = 10000;
            var synonymReader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadSynonymsFromID),
                -1);
            foreach (int id in pageInfo.UsedDefinitions)
            {
                synonymReader.SetParameter(id);
                List<string> synonyms = await synonymReader.GetData<string>();
                if (synonyms.Count == 0) continue;
                int synIndex = Ordinals.first;
                foreach (string synonym in synonyms)
                {
                    if (pageInfo.Query.Contains(synonym))
                    {
                        if (synIndex < lowestIndex)
                        {
                            lowestIndex = synIndex;
                            mainDefinition = id;
                            break;
                        }
                    }
                    synIndex++;
                }
            }
            synonymReader.Close();
            return mainDefinition;
        }

        private static async Task<Dictionary<string, int>> GetDefinitionHeadings(SearchPageInfo pageInfo)
        {
            Dictionary<string, int> headings = new Dictionary<string, int>();
            foreach (int id in pageInfo.UsedDefinitions)
            {
                string title = await ArticlePage.ReadTitle(id);
                int count = 1;
                string heading = title;
                while (headings.ContainsKey(heading))
                {
                    count++;
                    heading = title + ' ' + count;
                }
                headings.Add(heading, id);
            }
            return headings;
        }

        private static async Task WriteDefinitionHeadings(HTMLWriter writer, int mainDefinition, 
            Dictionary<string, int> headings, SearchPageInfo pageInfo)
        {
            int itemCount = Ordinals.first;
            bool active = false;
            foreach (KeyValuePair<string, int> entry in headings.OrderBy(e => e.Key))
            {
                int id = entry.Value;
                string word = entry.Key;
                await writer.Append("<li id=\"tabs0-");
                await writer.Append(itemCount);
                await writer.Append("\"");
                if (mainDefinition != Result.notfound)
                {
                    if (entry.Value == mainDefinition)
                    {
                        await writer.Append(" class=\"active\"");
                        active = true;
                    }
                }
                else
                if (!active)
                {
                    await writer.Append(" class=\"active\"");
                    active = true;
                }
                await writer.Append(">");
                await writer.Append(word);
                await writer.Append("</li>");
                itemCount++;
            }
            if (!string.IsNullOrEmpty(pageInfo.Query))
            {
                await writer.Append("<li id=\"tabs0-");
                await writer.Append(itemCount);
                await writer.Append("\"");
                if (!active) await writer.Append(" class='active'");
                await writer.Append(">");
                await writer.Append("Add New Article");
                await writer.Append("</li>");
            }
            await writer.Append("</ul></div><div class='definitions'><ul id=\"tabs-0-tab\" class=\"tab\">");
        }
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
                if (startID == -1) startID = sentenceKeywords[start].ID;
                int end = word.WordIndex + word.Length + 3;
                if (end >= searchresultwords.Count) end = searchresultwords.Count - 1;
                endID = sentenceKeywords[end].ID;
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
            citation.CitationType = CitationTypes.Text;
            await CitationConverter.AppendLink(writer, citation);
            await writer.Append(": ");
            bool ellipsis = false;

            for (int idx = Ordinals.first; idx<searchresultwords.Count; idx++)
            {
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