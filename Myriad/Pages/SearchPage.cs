using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.Data;
using Feliciana.ResponseWriter;
using Feliciana.HTML;
using Myriad.Library;
using Myriad.Data;
using Myriad.Search;
using Myriad.Parser;

namespace Myriad.Pages
{
    public class SearchPage : CommonPage
    {
        public const string pageURL = "/Search";
        public const string queryKeyQ = "q";
        public const string queryKeyIDs = "ids";
        CitationRange citationRange;
        string query;
        string all;
        List<string> idList;
        string ids;
        List<SearchSentence> results;
        List<int> usedDefinitions;
        //todo implement search page
        public override string GetURL()
        {
            return pageURL;
        }

        public override bool IsValid()
        {
            return (query != null) || (idList != null);
        }

        public override async Task LoadQueryInfo(IQueryCollection query)
        {
            if (query.ContainsKey(queryKeyQ))
            {
                string searchQuery = query["q"].ToString();
                (CitationRange r, string q) = SearchRange(searchQuery);
                citationRange = r;
                this.query = await AllWords.Conform(q);
            }
            else this.query = "";
            if (query.ContainsKey(queryKeyIDs))
            {
                ids = query["ids"].ToString();
                idList = ids.Split(Symbols.spaceArray, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else
            {
                ids = "";
            }
        }

        private (CitationRange r, string q) SearchRange(string query)
        {
            int p = query.IndexOf(':');
            if (p == Result.notfound)
            {
                return (CitationRange.InvalidRange(), query);
            }
            return (QueryToRange(query.Substring(0, p)), query.Substring(p + 1).Trim());
        }

        private CitationRange QueryToRange(string rangeString)
        {
            if (string.IsNullOrEmpty(rangeString)) return CitationRange.InvalidRange();
            string[] parts = rangeString.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            string[] startparts = parts[Ordinals.first].Split(Symbols.spaceArray, StringSplitOptions.RemoveEmptyEntries);
            (int startBook, int startChapter) = GetBookAndChapter(parts[Ordinals.first]);
            if (startBook == Result.error) return CitationRange.InvalidRange();
            int endBook = Result.error;
            int endChapter = Result.error;
            if (parts.Length > 1)
            {
                (int book, int chapter) = GetBookAndChapter(parts[Ordinals.second]);
                endBook = book;
                endChapter = chapter;
            }
            if (endBook == Result.error)
            {
                return StartCitationRange(startBook, startChapter);
            }
            return LongCitationRange(startBook, startChapter, endBook, endChapter);
        }

        private static CitationRange LongCitationRange(int startBook, int startChapter, int endBook, int endChapter)
        {
            KeyID startID = (startChapter == Result.error) ?
                new KeyID(startBook, 1, 1) :
                new KeyID(startBook, startChapter, 1);
            KeyID endID = (endChapter == Result.error) ?
                new KeyID(endBook, Bible.Chapters[endBook].Length - 1,
                    Bible.Chapters[endBook][Bible.Chapters[endBook].Length - 1],
                    KeyID.MaxWordIndex) :
                new KeyID(endBook, endChapter, 
                    Bible.Chapters[endBook][endChapter], 
                    KeyID.MaxWordIndex);
            return new CitationRange(startID, endID);
        }

        private static CitationRange StartCitationRange(int startBook, int startChapter)
        {
            KeyID startID = (startChapter == Result.error) ?
                new KeyID(startBook, 1, 1) :
                new KeyID(startBook, startChapter, 1);
            KeyID endID = (startChapter == Result.error) ?
                new KeyID(startBook,
                    Bible.Chapters[startBook].Length - 1,
                    Bible.Chapters[startBook][Bible.Chapters[startBook].Length - 1],
                    KeyID.MaxWordIndex) :
                new KeyID(startBook,
                    startChapter,
                    Bible.Chapters[startBook][startChapter],
                    KeyID.MaxWordIndex);
            return new CitationRange(startID, endID);
        }

        private (int book, int chapter) GetBookAndChapter(string citationString)
        {
            string[] parts = citationString.Split(Symbols.spaceArray, StringSplitOptions.RemoveEmptyEntries);
            if (!Bible.QueryBibleNames.ContainsKey(parts[Ordinals.first])) return (Result.error, Result.error);
            string bookString = Bible.QueryBibleNames[parts[Ordinals.first]];
            int book = Bible.IndexOfBook(bookString);
            if (book == Result.error) return (Result.error, Result.error);

            int chapter = (parts.Length > 1) ?
                Numbers.Convert(parts[Ordinals.second]) :
                Result.error;
            return (book, chapter);
        }

        protected override async Task WriteTitle(HTMLWriter writer)
        {
            await writer.Append("Search: ");
            await writer.Append(query);
        }

        protected override string PageScripts()
        {
            return TextHTML.TextScripts;
        }

        public async override Task RenderBody(HTMLWriter writer)
        {
            usedDefinitions = new List<int>();
            var phrases = await Phrases.GetPhrases(
                query.Split(Symbols.spaceArray, StringSplitOptions.RemoveEmptyEntries).ToList());
            (bool needSynonymQuery, List<List<string>> synonyms) =
                await SearchEvaluator.EvaluateSynonyms(phrases, usedDefinitions);
            results = await SearchEvaluator.Search(phrases, citationRange, synonyms, usedDefinitions);
            await FormatBody(writer);
        }

        internal async Task FormatBody(HTMLWriter writer)
        {
            await writer.Append("<a ID=querystring class=hidden href=");
            await writer.Append(query.Replace(' ', '_'));
            await writer.Append(">");
            await writer.Append(HTMLTags.EndAnchor);
            await writer.Append(HTMLTags.StartDivWithID);
            await writer.Append("definitionDiv");
            await writer.Append(HTMLTags.CloseQuote +
                HTMLTags.Class);
            await writer.Append("searchTabs");

            await AppendDefinitionsBlock(writer, usedDefinitions.Distinct().ToList(), idList, query);
            await writer.Append(HTMLTags.EndDiv);
            await SearchFormatter.AppendSearchResults(0, 100, writer, results);
            await writer.Append(HTMLTags.StartDivWithClass);
            await writer.Append("synresults");
            await writer.Append(HTMLTags.CloseQuoteEndTag +
                HTMLTags.EndDiv +
                HTMLTags.EndDiv);
            await AddPageTitleData(writer);
        }


        internal async Task AppendDefinitionsBlock(HTMLWriter writer, List<int> definitions, List<string> searchIDs, string query)
        { 
            string[] words = query.Split(Symbols.spaceArray, StringSplitOptions.RemoveEmptyEntries);
            //identify main definition
            int mainDefinition = Result.notfound;
            int lowestIndex = 10000;
            var synonymReader = new DataReaderProvider<int>(
                SqlServerInfo.GetCommand(DataOperation.ReadSynonymsFromID),
                -1);
            foreach (int id in definitions)
            {
                synonymReader.SetParameter(id);
                List<string> synonyms = await synonymReader.GetData<string>();
                if (synonyms.Count == 0) continue;
                int synIndex = Ordinals.first;
                foreach (string synonym in synonyms)
                {
                    if (query.Contains(synonym))
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
            //Add tabs
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
            bool active = false;
            int itemCount = Ordinals.first;
            Dictionary<string, int> headings = new Dictionary<string, int>();
            foreach (int id in definitions)
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
            if (!string.IsNullOrEmpty(query))
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
            active = false;
            itemCount = Ordinals.first;
            MarkupParser parser = new MarkupParser(writer);
            foreach (KeyValuePair<string, int> entry in headings.OrderBy(e => e.Key))
            {
                int id = entry.Value;
                string word = entry.Key;
                await writer.Append("<li id=\"tabs0-");
                await writer.Append(itemCount);
                await writer.Append("-tab\" ");
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
                    await writer.Append("class=\"active\"");
                    active = true;
                }
                var reader = new DataReaderProvider<int, int>(
                    SqlServerInfo.GetCommand(DataOperation.ReadArticleParagraph),
                    id, Ordinals.first);

                string definition = await reader.GetDatum<string>();
                reader.Close();
                if (string.IsNullOrEmpty(definition)) await writer.Append(">");
                else
                {
                    await writer.Append("><p class=\"definition\">");
                    parser.SetParagraphInfo(ParagraphType.Article, id);
                    await parser.ParseParagraph(definition, Ordinals.first);
                    await writer.Append("</p>");
                    List<Tuple<int, int>> usedParagraphs = new List<Tuple<int, int>>() { Tuple.Create(id, 0) };
                    foreach (int otherID in definitions)
                    {
                        if (otherID == id) continue;
                        var relatedReader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(
                            DataOperation.ReadRelatedParagraphIndex), id, otherID);
                        List<int> paragraphs = await reader.GetData<int>();
                        relatedReader.Close();
                        foreach (int paragraph in paragraphs)
                        {
                            Tuple<int, int> key = Tuple.Create(id, paragraph);
                            if (usedParagraphs.Contains(key)) continue;
                            usedParagraphs.Add(key);
                            await writer.Append("<p class=\"definition\">");
                            reader.SetParameter(id, paragraph);
                            await parser.ParseParagraph(await reader.GetDatum<string>(), paragraph);
                            await writer.Append("</p>");
                        }
                    }
                }
                reader.Close();
                await writer.Append("<p class=\"definitionnav\">");
                if ((!string.IsNullOrEmpty(definition)) && (!ids.Contains(id.ToString())))
                {
                    await writer.Append("<a HREF=" + SearchPage.pageURL + "?q=");
                    await writer.Append(query.Replace(' ', '+'));
                    await writer.Append("&ids=");
                    if (!string.IsNullOrEmpty(ids))
                    {
                        await writer.Append(ids);
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

        public override async Task AddTOC(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartList+
                HTMLTags.StartListItem+
                HTMLTags.ID);
            await writer.Append("link");
            await writer.Append(HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartAnchor +
                HTMLTags.HREF);
            await writer.Append("#top");
            await writer.Append(HTMLTags.EndTag);
            await writer.Append("Top of page");
            await writer.Append(HTMLTags.EndAnchor +
                HTMLTags.EndListItem +
                HTMLTags.EndList);
        }

        public override Task LoadTOCInfo(HttpContext context)
        {
            return Task.CompletedTask;
        }

        public override string GetQueryInfo()
        {
            return (string.IsNullOrEmpty(ids)) ?
                HTMLTags.StartQuery + queryKeyQ + query :
                HTMLTags.StartQuery + queryKeyQ + query +
                HTMLTags.Ampersand + queryKeyIDs + ids;
        }
    }
}
