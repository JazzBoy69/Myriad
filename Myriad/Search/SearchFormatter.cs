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

            await AppendSearchResults(0, 200, writer, pageInfo.SearchResults);

            await writer.Append(HTMLTags.StartDivWithID);
            await writer.Append("synresults"); 
            await writer.Append(HTMLTags.CloseQuoteEndTag +
                HTMLTags.EndDiv +
                HTMLTags.EndDiv);
        }

        private static async Task WriteInitialHTML(HTMLWriter writer, SearchPageInfo pageInfo)
        {
            await writer.Append("<div ID=querystring class=hidden>");
            await writer.Append(pageInfo.Query);
            await writer.Append(HTMLTags.EndDiv);
            await writer.Append(HTMLTags.StartDivWithID);
            await writer.Append("definitionDiv");
            await writer.Append(HTMLTags.CloseQuote +
                HTMLTags.Class+
                HTMLClasses.searchTabs+
                HTMLTags.CloseQuoteEndTag);
        }

        internal async static Task WriteDefinitionsBlock(HTMLWriter writer, SearchPageInfo pageInfo)
        {
            int mainDefinition = (pageInfo.UsedDefinitions.Count > Number.nothing) ?
                IdentifyMainDefinition(pageInfo) :
                Result.error;
            await StartDefinitionsTitles(writer);
            int itemCount = Ordinals.first;
            Dictionary<string, int> headings = (pageInfo.UsedDefinitions.Count > Number.nothing) ?
                await GetDefinitionHeadings(pageInfo) :
                new Dictionary<string, int>();
            await WriteDefinitionHeadings(writer, mainDefinition, headings, pageInfo);
            itemCount = Ordinals.first;
            MarkupParser parser = new MarkupParser(writer);
            await StartDefinitionTabs(writer);
            foreach (KeyValuePair<string, int> entry in headings.OrderBy(e => e.Key))
            {
                int id = entry.Value;
                bool setActive = (mainDefinition == Result.notfound) || (entry.Value == mainDefinition);
                if (setActive && (mainDefinition == Result.notfound)) mainDefinition = entry.Value;
                await StartDefinitionTab(writer, setActive, itemCount);
                string definition = await GetDefinition(id);
                if (!string.IsNullOrEmpty(definition))
                {
                    await WriteDefinition(writer, parser, id, definition);
                    List<(int id, int index)> usedParagraphs = new List<(int, int)>() { (id, 0) };
                    for (int i=Ordinals.first; i<pageInfo.UsedDefinitions.Count; i++)
                    {
                        if (pageInfo.UsedDefinitions[i] == id) continue;
                        parser.SetParagraphInfo(ParagraphType.Article, id);
                        List<int> relatedParagraphIndices = GetRelatedParagraphIndices(id, pageInfo.UsedDefinitions[i]);
                        for (int j=Ordinals.first; j<relatedParagraphIndices.Count; j++)
                        {
                            (int id, int index) key = (id, relatedParagraphIndices[j]);
                            if (usedParagraphs.Contains(key)) continue;
                            usedParagraphs.Add(key);
                            await WriteRelatedParagraph(writer, parser, id, relatedParagraphIndices[j]);
                        }
                    }
                }
                await writer.Append("<p class=\"definitionnav\">");
                if ((!string.IsNullOrEmpty(definition)) &&
                  ((pageInfo.IDs == null) || (!pageInfo.IDs.Contains(id.ToString()))))
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
                    await AppendPartialPageHandler(writer);
                    await writer.Append(">Search&nbsp;for&nbsp;this&nbsp;definition</a> ");
                }
                await writer.Append("<a HREF=" + ArticlePage.pageURL + "?q=");
                await writer.Append(pageInfo.Query.Replace(' ', '+'));
                await writer.Append("&id=");
                await writer.Append(id);
                await AppendPartialPageHandler(writer);
                await writer.Append(">Read&nbsp;more&hellip;</a></p>");
                await writer.Append("</li>");
                itemCount++;
            }
            if (!string.IsNullOrEmpty(pageInfo.Query))
            {
                bool setActive = (mainDefinition == Result.notfound);
                await StartDefinitionTab(writer, setActive, itemCount);
                string[] words = pageInfo.Query.Split(Symbols.spaceArray, StringSplitOptions.RemoveEmptyEntries);
                for (int i=Ordinals.first; i<words.Length; i++)
                {
                    await writer.Append("<p class=\"definitionnav\"> ");
                    await writer.Append(HTMLTags.StartAnchor +
                        HTMLTags.HREF +
                        ArticlePage.addArticleURL +
                        HTMLTags.StartQuery +
                        ArticlePage.queryKeyTitle +
                        Symbol.equal);
                    await writer.Append(words[i]);
                    await writer.Append(HTMLTags.OnClick +
                        JavaScriptFunctions.HandleLink);
                    await writer.Append(">Add Article for ");
                    await writer.Append(HTMLTags.StartBold);
                    await writer.Append(words[i].Replace('_', ' '));
                    await writer.Append(HTMLTags.EndBold+
                        HTMLTags.EndAnchor+
                        HTMLTags.EndParagraph);
                }


                await writer.Append("</li>");
            }
            await writer.Append("</ul></div></section>");
        }

        private async static Task<string> GetDefinition(int id)
        {
            string definition = "[[";
            int index = Ordinals.first;
            while (definition[Ordinals.first] == '[')
            {
                definition = await GetArticleParagraph(id, index);
                index++;
            }
            return definition;
        }

        private static async Task StartDefinitionTabs(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartDivWithClass +
                            HTMLClasses.definitions +
                            HTMLTags.CloseQuoteEndTag +
                            HTMLTags.StartList +
                            HTMLTags.ID);
            await writer.Append("tabs-0-tab");
            await writer.Append(HTMLTags.Class+HTMLClasses.tab+
                HTMLTags.CloseQuoteEndTag);
        }

        private static async Task WriteRelatedParagraph(HTMLWriter writer, MarkupParser parser, int id, int paragraphIndex)
        {
            string paragraphText = await GetArticleParagraph(id, paragraphIndex);
            if (paragraphText == null) return;
            await writer.Append("<p class=\"definition\">");
            await parser.ParseParagraph(paragraphText, paragraphIndex);
            await writer.Append("</p>");
        }

        private static List<int> GetRelatedParagraphIndices(int id, int otherID)
        {
            var relatedReader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(
                            DataOperation.ReadRelatedParagraphIndex), id, otherID);
            List<int> paragraphs = relatedReader.GetData<int>();
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
            var reader = new StoredProcedureProvider<int, int>(
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
            await writer.Append(HTMLClasses.definitionsheader);
            await writer.Append(HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartList +
                HTMLTags.ID);
            await writer.Append("tabs0");
            await writer.Append(HTMLTags.CloseQuote +
                HTMLTags.Class);
            await writer.Append("tabs");
            await writer.Append(HTMLTags.CloseQuoteEndTag);
        }

        private static int IdentifyMainDefinition(SearchPageInfo pageInfo)
        {
            int mainDefinition = Result.notfound;
            int lowestIndex = 10000;
            var synonymReader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadSynonymsFromID),
                -1);
            for (int i=Ordinals.first; i<pageInfo.UsedDefinitions.Count; i++)
            {
                synonymReader.SetParameter(pageInfo.UsedDefinitions[i]);
                List<string> synonyms = synonymReader.GetData<string>();
                if (synonyms.Count == 0) continue;
                for (int synIndex = Ordinals.first; synIndex < synonyms.Count; synIndex++)
                {
                    if (pageInfo.Query.Contains(synonyms[synIndex]))
                    {
                        if (synIndex < lowestIndex)
                        {
                            lowestIndex = synIndex;
                            mainDefinition = pageInfo.UsedDefinitions[i];
                            break;
                        }
                    }
                }
            }
            synonymReader.Close();
            return mainDefinition;
        }

        private static async Task<Dictionary<string, int>> GetDefinitionHeadings(SearchPageInfo pageInfo)
        {
            Dictionary<string, int> headings = new Dictionary<string, int>();
            for (int i=Ordinals.first; i<pageInfo.UsedDefinitions.Count; i++)
            {
                string title = await Reader.ReadTitle(pageInfo.UsedDefinitions[i]);
                int count = 1;
                string heading = title;
                while (headings.ContainsKey(heading))
                {
                    count++;
                    heading = title + ' ' + count;
                }
                headings.Add(heading, pageInfo.UsedDefinitions[i]);
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
                await writer.Append("<li id='tabs0-");
                await writer.Append(itemCount);
                await writer.Append("'");
                if (mainDefinition != Result.notfound)
                {
                    if (entry.Value == mainDefinition)
                    {
                        await writer.Append(HTMLTags.Class +
                            HTMLClasses.active +
                            HTMLTags.CloseQuote);
                        active = true;
                    }
                }
                else
                if (!active)
                {
                    await writer.Append(HTMLTags.Class+
                        HTMLClasses.active+
                        HTMLTags.CloseQuote);
                    active = true;
                }
                await writer.Append(HTMLTags.OnClick);
                await writer.Append(JavaScriptFunctions.HandleDefinitionClick);
                await writer.Append(HTMLTags.EndTag);
                await writer.Append(word);
                await writer.Append(HTMLTags.EndListItem);
                itemCount++;
            }
            if (!string.IsNullOrEmpty(pageInfo.Query))
            {
                await writer.Append("<li id='tabs0-");
                await writer.Append(itemCount);
                await writer.Append("'");
                await writer.Append(HTMLTags.OnClick +
                    JavaScriptFunctions.HandleDefinitionClick);
                if (!active)
                {
                    await writer.Append(HTMLTags.Class +
                        HTMLClasses.active +
                        HTMLTags.CloseQuote);
                    active = true;
                }
                await writer.Append(HTMLTags.EndTag);
                await writer.Append("Add New Article");
                await writer.Append(HTMLTags.EndListItem);
            }
            await writer.Append(HTMLTags.EndList + HTMLTags.EndDiv);
        }

        public static async Task AppendSearchResults(int startIndex, int endIndex, HTMLWriter writer, List<SearchSentence> searchResults)
        {
            if ((searchResults == null) ||
                ((searchResults.Count == 1) && (searchResults[0].SentenceID == Result.notfound)))
            {
                return;
            }

            var usedSentences = new List<int>();
            for (int index = startIndex; (index <= endIndex) && (index < searchResults.Count); index++)
            {
                if (usedSentences.Contains(searchResults[index].SentenceID)) continue;
                await writer.Append(HTMLTags.StartParagraph);
                await FormatSearchResult(writer, searchResults[index]);
                await writer.Append(HTMLTags.EndParagraph);
                usedSentences.Add(searchResults[index].SentenceID);
            }
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
            List<Keyword> sentenceKeywords = reader.GetClassData<Keyword>();
            reader.Close();
            List<SearchResultWord> searchresultwords = new List<SearchResultWord>();
            for (int index = Ordinals.first; index < sentenceKeywords.Count; index++)
            {
                searchresultwords.Add(new SearchResultWord());
            }
            if (searchresultwords.Count == Number.nothing) return;
            var links = new Dictionary<int, (int, int)>();
            List<int> endLinks = new List<int>();
            for (int i=Ordinals.first; i<searchSentence.Words.Count; i++)
            {
                int start = searchSentence.Words[i].WordIndex - 4;
                if (start < 0) start = Ordinals.first;
                if (startID == -1) startID = sentenceKeywords[start].ID;
                int end = searchSentence.Words[i].WordIndex + searchSentence.Words[i].Length + 3;
                if (end >= searchresultwords.Count) end = searchresultwords.Count - 1;
                endID = sentenceKeywords[end].ID;
                int highlight = searchSentence.Words[i].WordIndex + searchSentence.Words[i].Length - 1;
                if (highlight >= searchresultwords.Count) highlight = searchresultwords.Count - 1;
                if (searchSentence.Words[i].ArticleID != -1)
                {
                    highlight = await ReadDefinitionSearchLength(sentenceID, searchSentence.Words[i].WordIndex,
                        searchSentence.Words[i].ArticleID);
                    if (links.ContainsKey(searchSentence.Words[i].WordIndex))
                    {
                        if (highlight > links[searchSentence.Words[i].WordIndex].Item2)
                        {
                            endLinks.Remove(links[searchSentence.Words[i].WordIndex].Item2);
                            links[searchSentence.Words[i].WordIndex] = (searchSentence.Words[i].ArticleID, highlight);
                            endLinks.Add(highlight);
                        }
                    }
                    else
                    {
                        links.Add(searchSentence.Words[i].WordIndex, (searchSentence.Words[i].ArticleID, highlight));
                        endLinks.Add(highlight);
                    }
                }
                if (searchSentence.Words[i].Substitute)
                {
                    searchSentence.Words[i].Length = await ReadSubtituteLength(sentenceID, searchSentence.Words[i].WordIndex);
                    if (searchSentence.Words[i].Length > 1) searchresultwords[searchSentence.Words[i].WordIndex].Length = 
                            searchSentence.Words[i].Length;
                    highlight = searchSentence.Words[i].WordIndex;
                    if (searchresultwords[searchSentence.Words[i].WordIndex].IsMainText)
                        searchresultwords[searchSentence.Words[i].WordIndex].SubstituteText = searchSentence.Words[i].Text;
                    else
                        searchresultwords[searchSentence.Words[i].WordIndex].SubstituteText = 
                            searchSentence.Words[i].Text.Replace('[', '(').Replace(']', ')');
                    if (searchSentence.Words[i].Length > 1)
                    {
                        int wordIndex = searchSentence.Words[i].WordIndex + 1;
                        while ((wordIndex < searchSentence.Words[i].WordIndex + searchSentence.Words[i].Length) && 
                            (wordIndex < searchresultwords.Count))
                        {
                            searchresultwords[wordIndex].Erased = true;
                            wordIndex++;
                        }
                    }
                }
                int index = start;
                while (index < searchSentence.Words[i].WordIndex)
                {
                    if (searchSentence.Words[i].WordIndex >= searchresultwords.Count) break;
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
            bool link = false;
            for (int idx = Ordinals.first; idx<searchresultwords.Count; idx++)
            {
                if (searchresultwords[idx].Erased)
                {
                    //if (endLinks.Contains(idx)) await writer.Append(HTMLTags.EndAnchor);
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
                if (searchresultwords[idx].Erased) continue;
                ellipsis = false;
                if (!link && ((searchresultwords[idx].Highlight) || (searchresultwords[idx].Substituted)))
                    await writer.Append(HTMLTags.StartBold);
                await writer.Append(sentenceKeywords[idx].LeadingSymbolString);
                if (links.ContainsKey(idx))
                {
                    await AppendSearchArticle(writer, startID, endID, links[idx].Item1);
                    link = true;
                }
                if (searchresultwords[idx].Substituted)
                {
                    if (searchresultwords[idx].IsMainText)
                        await writer.Append("[");
                    else
                        await writer.Append("(");
                }
                await AppendSearchResultWord(writer, searchresultwords[idx], sentenceKeywords[idx]);
                if (searchresultwords[idx].Substituted)
                {
                    if (searchresultwords[idx].IsMainText)
                        await writer.Append("]");
                    else
                        await writer.Append(")");
                }
                if (endLinks.Contains(idx + searchresultwords[idx].Length - 1))
                {
                    await writer.Append(HTMLTags.EndAnchor);
                    link = false;
                }
                await writer.Append(sentenceKeywords[idx].TrailingSymbolString);
                if (!link && ((searchresultwords[idx].Highlight) || (searchresultwords[idx].Substituted)))
                    await writer.Append(HTMLTags.EndBold);
            }
        }

        private static async Task<int> ReadSubtituteLength(int sentenceID, int wordIndex)
        {
            StoredProcedureProvider<int, int> reader = new StoredProcedureProvider<int, int>(SqlServerInfo.GetCommand(
                DataOperation.ReadSubstituteLength), sentenceID, wordIndex);
            int result = await reader.GetDatum<int>();
            reader.Close();
            return result;
        }

        private static async Task<int> ReadDefinitionSearchLength(int sentenceID, int wordIndex, int articleID)
        {
            DataReaderProvider<int, int, int> reader = new DataReaderProvider<int, int, int>(SqlServerInfo.GetCommand(
                DataOperation.ReadDefinitionSearchLength), sentenceID, wordIndex, articleID);
            int result = await reader.GetDatum<int>();
            reader.Close();
            return result;
        }

        internal static async Task AppendSearchArticle(HTMLWriter writer, int startID, int endID, int id)
        {
            await writer.Append("<a HREF=" + ArticlePage.pageURL + "?id=");
            await writer.Append(id);
            await writer.Append("&tgstart=");
            await writer.Append(startID);
            await writer.Append("&tgend=");
            await writer.Append(endID);
            await AppendPartialPageHandler(writer);
            await writer.Append(">");
        }

        private static async Task AppendPartialPageHandler(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.Ampersand +
                HTMLClasses.partial +
                HTMLTags.OnClick +
            JavaScriptFunctions.HandleLink);
        }

        public static async Task AppendSearchResultWord(HTMLWriter writer, SearchResultWord searchResultWord, Keyword keyword)
        {
            if (searchResultWord.Erased) return;
            if (searchResultWord.Substituted)
            {
                if (keyword.IsCapitalized)
                {
                    await writer.Append(Symbols.Capitalize(searchResultWord.SubstituteText).Replace('_', ' '));
                }
                else
                {
                    await writer.Append(searchResultWord.SubstituteText.Replace('_', ' '));
                }
                return;
            }
            if (searchResultWord.Highlight)
            {
                await AppendTextOfSearchKeyword(writer, keyword);
                return;
            }
            if (searchResultWord.Used)
            {
                await AppendTextOfSearchKeyword(writer, keyword);
            }
        }
        public async static Task AppendTextOfSearchKeyword(HTMLWriter writer, Keyword keyword)
        {
            if (keyword.IsCapitalized)
            {
                await writer.Append(keyword.Text.Slice(Ordinals.first, 1).ToString().ToUpperInvariant());
                string text = keyword.Text.Slice(Ordinals.second).ToString().Replace('`', '’');
                text = text.Replace("΄", HTMLClasses.startExtraInfo + "΄" + HTMLTags.EndSpan).Replace("·", HTMLClasses.startExtraInfo + "·" + HTMLTags.EndSpan);
                await writer.Append(text);
            }
            else
            {
                string text = keyword.Text.ToString().Replace('`', '’');
                text = text.Replace("΄", HTMLClasses.startExtraInfo + "΄" + HTMLTags.EndSpan).Replace("·", HTMLClasses.startExtraInfo + "·" + HTMLTags.EndSpan);
                await writer.Append(text);
            }
        }
    }
}