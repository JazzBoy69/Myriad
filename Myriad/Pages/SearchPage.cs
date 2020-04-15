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

        SearchPageInfo pageInfo = new SearchPageInfo();

        //todo implement search page
        public override string GetURL()
        {
            return pageURL;
        }

        public override bool IsValid()
        {
            return (pageInfo.Query != null) || (pageInfo.IDList != null);
        }

        public override async Task LoadQueryInfo(IQueryCollection query)
        {
            if (query.ContainsKey(queryKeyQ))
            {
                string searchQuery = query["q"].ToString();
                (CitationRange r, string q) = SearchRange(searchQuery);
                pageInfo.SetCitationRange(r);
                pageInfo.SetQuery(await AllWords.Conform(q));
            }
            if (query.ContainsKey(queryKeyIDs))
            {
                pageInfo.SetIDs(query["ids"].ToString());
  
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
            await writer.Append(pageInfo.Query);
        }

        protected override string PageScripts()
        {
            return TextHTML.TextScripts;
        }

        public async override Task RenderBody(HTMLWriter writer)
        {
            var phrases = await Phrases.GetPhrases(pageInfo.QueryWords);
            var searchEvaluator = new SearchEvaluator();
            await searchEvaluator.EvaluateSynonyms(phrases);
            var results = await searchEvaluator.Search(phrases, pageInfo.CitationRange);
            pageInfo.SetResults(results);
            pageInfo.SetUsedDefinitions(searchEvaluator.UsedDefinitions);
            await SearchFormatter.FormatBody(writer, pageInfo);
            await AddPageTitleData(writer);
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
            return (string.IsNullOrEmpty(pageInfo.IDs)) ?
                HTMLTags.StartQuery + queryKeyQ + pageInfo.Query :
                HTMLTags.StartQuery + queryKeyQ + pageInfo.Query +
                HTMLTags.Ampersand + queryKeyIDs + pageInfo.IDs;
        }
    }
}
