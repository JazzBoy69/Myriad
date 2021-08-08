using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Feliciana.Library;
using Feliciana.ResponseWriter;
using Feliciana.Data;
using Myriad.Library;
using Myriad.Parser;
using Myriad.Data;
using System.Runtime.CompilerServices;
using Myriad.Search;

namespace Myriad.Pages
{
    internal class EditParagraph
    {
        internal static string getDataURL = "/EditParagraph/GetData";
        internal static string setDataURL = "/EditParagraph/SetData";
        static readonly Dictionary<ParagraphType, Func<MarkupParser, ArticleParagraph, Task>> updateMethods =
            new Dictionary<ParagraphType, Func<MarkupParser, ArticleParagraph, Task>>()
            {
                {ParagraphType.Article, UpdateArticleParagraph },
                {ParagraphType.Comment, UpdateCommentParagraph },
                {ParagraphType.Navigation, UpdateNavigationParagraph },
                {ParagraphType.Chrono, UpdateChronoParagraph }
            };
        internal static async Task GetPlainText(HttpContext context)
        {
            ParagraphInfo info = new ParagraphInfo(context);
            await context.Response.WriteAsync(await info.Text());      
        }

        internal static async Task SetText(HttpContext context)
        {
            ParagraphInfo info = new ParagraphInfo(context);
            if (info.type == ParagraphType.Undefined) return;
            context.Request.Form.TryGetValue("text", out var text);
            MarkupParser parser = new MarkupParser(Writer.New(context.Response));
            ArticleParagraph paragraph = new ArticleParagraph(info.ID, info.index, text);
            await updateMethods[info.type].Invoke(parser, paragraph);
        }
        public static async Task UpdateArticleParagraph(MarkupParser parser, ArticleParagraph paragraph)
        {
            await DataRepository.DeleteGlossaryParagraph(paragraph.ID, paragraph.ParagraphIndex);
            await DataRepository.WriteGlossaryParagraph(paragraph.ID, paragraph.ParagraphIndex, paragraph.Text);
           
            await parser.ParseParagraph(paragraph.Text, paragraph.ParagraphIndex);
            var citations = await CitationConverter.ResolveCitations(parser.Citations);
            if (paragraph.Text.Length > 1)
            {
                string token = paragraph.Text.Substring(Ordinals.first, 2);
                if ((token == "[|") || (token == "|-") || (token == "||")) 
                    citations = new List<Citation>();
            }
            await AddDefinitionSearches(paragraph, citations);
            await AddRelatedArticles(paragraph, citations);
            await AddRelatedTags(parser.Tags, paragraph);
        }

        internal static async Task AddDefinitionSearches(ArticleParagraph paragraph, List<Citation> citationsToAdd)
        {
            List<string> synonyms = await DataRepository.Synonyms(paragraph.ID);
            for (int index = Ordinals.first; index < citationsToAdd.Count; index++)
            {
                List<SearchWord> searchWords = await DataRepository.SearchWords(synonyms, citationsToAdd[index].Start, citationsToAdd[index].End);
                await WriteDefinitionSearches(paragraph, searchWords);
            }
        }

        private static async Task WriteDefinitionSearches(ArticleParagraph paragraph, List<SearchWord> searchWords)
        {
            if (searchWords.Count == Number.nothing) return;
            List<DefinitionSearch> definitionSearches = new List<DefinitionSearch>();
            for (int index = Ordinals.first; index < searchWords.Count; index++)
            {
                var search = new DefinitionSearch();
                search.SetData(searchWords[index], paragraph.ID, paragraph.ParagraphIndex);
                definitionSearches.Add(search);
            }
            await DataRepository.WriteDefinitionSearches(definitionSearches);
        }
        private static async Task AddRelatedTags(List<string> tags, ArticleParagraph paragraph)
        {
            List<int> relatedIDs = await GetRelatedIDs(tags);
            await DataRepository.WriteRelatedTags(paragraph.ID, paragraph.ParagraphIndex, relatedIDs);
        }

        private static async Task AddRelatedArticles(ArticleParagraph paragraph, List<Citation> citationsToAdd)
        {
            var linksToAdd =
                            CitationConverter.ToCrossReferences(citationsToAdd, paragraph.ID, paragraph.ParagraphIndex);
            await DataRepository.WriteRelatedArticles(linksToAdd);
        } 

        private static async Task<List<int>> GetRelatedIDs(List<string> tags)
        {
            List<int> result = new List<int>();
            for (int index = Ordinals.first; index < tags.Count; index++)
            {
                result.Add(await DataRepository.ArticleID(tags[index])); //todo search for identifier and synonyms also
            }
            return result;
        }

        public static async Task UpdateCommentParagraph(MarkupParser parser, ArticleParagraph paragraph)
        {
            await DataRepository.DeleteCommentParagraph(paragraph.ID, paragraph.ParagraphIndex);
            await DataRepository.WriteCommentParagraph(paragraph.ID, paragraph.ParagraphIndex, paragraph.Text);
            await parser.ParseParagraph(paragraph.Text, paragraph.ParagraphIndex);
            var citations = await CitationConverter.ResolveCitations(parser.Citations);
            if (paragraph.Text.Length > 1)
            {
                string token = paragraph.Text.Substring(Ordinals.first, 2);
                if ((token == "[|") || (token == "|-") || (token == "||")) citations = new List<Citation>();
            }
            var linksToAdd =
                CitationConverter.ToCrossReferences(citations, paragraph.ID, paragraph.ParagraphIndex);
            await DataRepository.WriteCrossReferences(linksToAdd);
        }


        internal async static Task AddCommentParagraph(MarkupParser parser, ArticleParagraph paragraph)
        {
            await DataRepository.WriteCommentParagraph(paragraph.ID, paragraph.ParagraphIndex, paragraph.Text);
            await parser.ParseParagraph(paragraph.Text, paragraph.ParagraphIndex);
            var citations = await CitationConverter.ResolveCitations(parser.Citations);
            if (paragraph.Text.Length > 1)
            {
                string token = paragraph.Text.Substring(Ordinals.first, 2);
                if ((token == "[|") || (token == "|-") || (token == "||")) citations = new List<Citation>();
            }
            var tags = parser.Tags;
            var crossReferencesToAdd =
                CitationConverter.ToCrossReferences(citations, paragraph.ID, paragraph.ParagraphIndex);
            await DataRepository.WriteCrossReferences(crossReferencesToAdd);
        }

        internal async static Task AddArticleParagraph(PageParser parser, ArticleParagraph paragraph)
        {
            await DataRepository.WriteGlossaryParagraph(paragraph.ID, paragraph.ParagraphIndex, paragraph.Text);
            await parser.ParseParagraph(paragraph.Text, paragraph.ParagraphIndex);
            var citations = await CitationConverter.ResolveCitations(parser.Citations);
            if (paragraph.Text.Length > 1)
            {
                string token = paragraph.Text.Substring(Ordinals.first, 2);
                if ((token == "[|") || (token == "|-") || (token == "||"))
                    citations = new List<Citation>();
            }
            var tags = parser.Tags;
            var crossReferencesToAdd =
                CitationConverter.ToCrossReferences(citations, paragraph.ID, paragraph.ParagraphIndex);
            await DataRepository.WriteRelatedArticles(crossReferencesToAdd);
            await AddDefinitionSearches(paragraph, citations);
            List<int> relatedIDs = await GetRelatedIDs(parser.Tags);
            var tagsToAdd = new List<(int ArticleID, int ParagraphIndex, int RelatedID)>();
            for (int index = Ordinals.first; index < relatedIDs.Count; index++)
            {
                tagsToAdd.Add((paragraph.ID, paragraph.ParagraphIndex, relatedIDs[index]));
            }
            await DataRepository.WriteRelatedTags(tagsToAdd);
        }

        public static async Task UpdateNavigationParagraph(MarkupParser parser, ArticleParagraph paragraph)
        {
            await DataRepository.DeleteNavigationParagraph(paragraph.ID, paragraph.ParagraphIndex);
            await DataRepository.WriteNavigationParagraph(paragraph.ID, paragraph.ParagraphIndex, paragraph.Text);
        }

        public static async Task UpdateChronoParagraph(MarkupParser parser, ArticleParagraph paragraph)
        {
            await DataRepository.DeleteCommentChapterParagraph(paragraph.ID, paragraph.ParagraphIndex);
            await DataRepository.WriteCommentChapterParagraph(paragraph.ID, paragraph.ParagraphIndex, paragraph.Text);
        }
    }
}