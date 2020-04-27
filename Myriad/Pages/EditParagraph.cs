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
        internal static async Task GetPlainText(HttpContext context)
        {
            context.Request.Form.TryGetValue("edittype", out var editType);
            ParagraphType paragraphType = (ParagraphType)Convert.ToInt32(editType);
            context.Request.Form.TryGetValue("ID", out var ID);
            int articleID = Convert.ToInt32(ID);
            context.Request.Form.TryGetValue("paragraphIndex", out var index);
            int paragraphIndex = Convert.ToInt32(index);
            switch (paragraphType)
            {
                case ParagraphType.Article:
                    await SendPlainTextParagraph(DataOperation.ReadArticleParagraph, articleID, paragraphIndex, context.Response);
                    break;
                case ParagraphType.Comment:
                    await SendPlainTextParagraph(DataOperation.ReadCommentParagraph, articleID, paragraphIndex, context.Response);
                    break;
                case ParagraphType.Navigation:
                    await SendPlainTextParagraph(DataOperation.ReadNavigationParagraph, articleID, paragraphIndex, context.Response);
                    break;
                case ParagraphType.Undefined:
                    break;
                default:
                    break;
            }
        }

        private static async Task SendPlainTextParagraph(DataOperation operation, int articleID, int paragraphIndex, HttpResponse response)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(operation), articleID, paragraphIndex);
            await response.WriteAsync(await reader.GetDatum<string>());
            reader.Close();
        }

        internal static async Task SetText(HttpContext context)
        {
            context.Request.Form.TryGetValue("edittype", out var editType);
            ParagraphType paragraphType = (ParagraphType)Convert.ToInt32(editType);
            context.Request.Form.TryGetValue("ID", out var ID);
            int articleID = Convert.ToInt32(ID);
            context.Request.Form.TryGetValue("paragraphIndex", out var index);
            int paragraphIndex = Convert.ToInt32(index);
            context.Request.Form.TryGetValue("text", out var text);
            MarkupParser parser = new MarkupParser(Writer.New(context.Response));
            ArticleParagraph paragraph = new ArticleParagraph(articleID, paragraphIndex, text);
            switch (paragraphType)
            {
                case ParagraphType.Article:
                    await UpdateArticleParagraph(parser, paragraph);
                    break;
                case ParagraphType.Comment:
                    await UpdateCommentParagraph(parser, paragraph);
                    break;
                case ParagraphType.Navigation:
                    await UpdateNavigationParagraph(parser, paragraph);
                    break;
                default:
                    return;
            }
        }
        public static async Task UpdateArticleParagraph(MarkupParser parser, ArticleParagraph paragraph)
        {
            await UpdateArticleParagraphInDatabase(paragraph);
            await parser.ParseParagraph(paragraph.Text, paragraph.ParagraphIndex);
            var citations = parser.Citations;
            var oldCitations = await ReadRelatedArticleLinks(paragraph.ID, paragraph.ParagraphIndex);
            (List<Citation> citationsToAdd, List<Citation> citationsToDelete) =
                CompareCitationLists(citations, oldCitations);
            await AddRelatedArticles(paragraph, citationsToAdd);
            await AddDefinitionSearches(paragraph, citationsToAdd);
            await DeleteRelatedArticles(paragraph, citationsToDelete);
            await DeleteDefinitionSearches(paragraph, citationsToDelete);
            await UpdateRelatedTags(parser.Tags, paragraph);
        }

        private static async Task DeleteDefinitionSearches(ArticleParagraph paragraph, List<Citation> citationsToDelete)
        {
            var reader = new DataReaderProvider<int, int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadDefinitionSearchIDs),
                -1, -1, -1);
            for (int index = Ordinals.first; index < citationsToDelete.Count; index++)
            {
                reader.SetParameter(paragraph.ID, citationsToDelete[index].CitationRange.StartID.ID,
                    citationsToDelete[index].CitationRange.EndID.ID);
                List<int> ids = reader.GetData<int>();
                if (ids.Count > Number.nothing)
                {
                    await DataWriterProvider.Write(SqlServerInfo.GetCommand(DataOperation.DeleteDefinitionSearch), ids);
                }
            }
        }

        private static async Task AddDefinitionSearches(ArticleParagraph paragraph, List<Citation> citationsToAdd)
        {
            List<string> synonyms = ArticlePage.GetSynonyms(paragraph.ID);
            for (int index = Ordinals.first; index < citationsToAdd.Count; index++)
            {
                List<SearchWord> searchWords = ReadSearchWords(synonyms, citationsToAdd[index].CitationRange);
                await WriteDefinitionSearches(paragraph.ID, searchWords);
            }
        }

        private static async Task WriteDefinitionSearches(int id, List<SearchWord> searchWords)
        {
            if (searchWords.Count == Number.nothing) return;
            List<DefinitionSearch> definitionSearches = new List<DefinitionSearch>();
            for (int index = Ordinals.first; index < searchWords.Count; index++)
            {
                definitionSearches.Add(new DefinitionSearch(searchWords[index], id));
            }
            await DataWriterProvider.WriteData(
                SqlServerInfo.GetCommand(DataOperation.CreateDefinitionSearch), definitionSearches);
        }

        private static List<SearchWord> ReadSearchWords(List<string> synonyms, CitationRange citationRange)
        {
            var reader = new DataReaderProvider<string, int, int>(SqlServerInfo.GetCommand(DataOperation.ReadSearchWords),
                "", citationRange.StartID.ID, citationRange.EndID.ID);
            var searchWords = new List<SearchWord>();
            for (int index = Ordinals.first; index < synonyms.Count; index++)
            {
                reader.SetParameter(synonyms[index]);
                searchWords.AddRange(reader.GetClassData<SearchWord>());
            }
            reader.Close();
            return searchWords;
        }

        private static async Task UpdateRelatedTags(List<string> tags, ArticleParagraph paragraph)
        {
            List<int> relatedIDs = await GetRelatedIDs(tags);
            List<int> oldRelatedIDs = ReadExistingRelatedIDs(paragraph);
            (List<RelatedTag> tagsToAdd, List<RelatedTag> tagsToDelete) = CompareIDLists(relatedIDs, oldRelatedIDs, paragraph);
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.CreateRelatedTags),
                tagsToAdd);
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.DeleteRelatedTags),
                tagsToDelete);
        }

        private static async Task DeleteRelatedArticles(ArticleParagraph paragraph, List<Citation> citationsToDelete)
        {
            List<CrossReference> linksToDelete =
                            await CitationConverter.ToCrossReferences(citationsToDelete, paragraph.ID, paragraph.ParagraphIndex);
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.DeleteRelatedArticleLinks),
                linksToDelete);
        }

        private static async Task AddRelatedArticles(ArticleParagraph paragraph, List<Citation> citationsToAdd)
        {
            List<CrossReference> linksToAdd =
                            await CitationConverter.ToCrossReferences(citationsToAdd, paragraph.ID, paragraph.ParagraphIndex);
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.CreateRelatedArticleLinks),
                linksToAdd);
        }

        private static async Task UpdateArticleParagraphInDatabase(ArticleParagraph paragraph)
        {
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.UpdateArticleParagraph),
                paragraph);
        }

        private static (List<RelatedTag> tagsToAdd, List<RelatedTag> tagsToDelete) CompareIDLists(List<int> newIDs, List<int> oldIDs, ArticleParagraph paragraph)
        {
            List<RelatedTag> tagsToAdd = new List<RelatedTag>();
            for (int index = Ordinals.first; index < newIDs.Count; index++)
            {
                if (!oldIDs.Contains(newIDs[index]))
                {
                    tagsToAdd.Add(new RelatedTag(paragraph.ID, paragraph.ParagraphIndex, newIDs[index]));
                }
            }
            List<RelatedTag> tagsToDelete = new List<RelatedTag>();
            for (int index = Ordinals.first; index < oldIDs.Count; index++)
            {
                if (!newIDs.Contains(oldIDs[index]))
                {
                    tagsToDelete.Add(new RelatedTag(paragraph.ID, paragraph.ParagraphIndex, oldIDs[index]));
                }
            }
            return (tagsToAdd, tagsToDelete);
        }

        private static List<int> ReadExistingRelatedIDs(ArticleParagraph paragraph)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadExistingRelatedIDs),
                paragraph.ID, paragraph.ParagraphIndex);
            var result = reader.GetData<int>();
            reader.Close();
            return result;
        }

        private static async Task<List<int>> GetRelatedIDs(List<string> tags)
        {
            var reader = new DataReaderProvider<string>(SqlServerInfo.GetCommand(DataOperation.ReadArticleID),
                "");
            List<int> result = new List<int>();
            for (int index = Ordinals.first; index < tags.Count; index++)
            {
                reader.SetParameter(tags[index]);
                result.Add(await reader.GetDatum<int>());
            }
            reader.Close();
            return result;
        }

        public static async Task UpdateCommentParagraph(MarkupParser parser, ArticleParagraph paragraph)
        {
            await WriteParagraphToDatabase(paragraph);
            await parser.ParseParagraph(paragraph.Text, paragraph.ParagraphIndex);
            var citations = parser.Citations;
            var oldCitations = await ReadCrossReferences(paragraph.ID, paragraph.ParagraphIndex);
            (List<Citation> citationsToAdd, List<Citation> citationsToDelete) =
                CompareCitationLists(citations, oldCitations);
            List<CrossReference> linksToAdd =
                await CitationConverter.ToCrossReferences(citationsToAdd, paragraph.ID, paragraph.ParagraphIndex);
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.CreateCrossReferences),
                linksToAdd);
            List<CrossReference> linksToDelete =
                await CitationConverter.ToCrossReferences(citationsToDelete, paragraph.ID, paragraph.ParagraphIndex);
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.DeleteCrossReferences),
                linksToDelete);
        }

        public static async Task WriteParagraphToDatabase(ArticleParagraph paragraph)
        {
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.UpdateCommentParagraph),
                            paragraph);
        }

        internal async static Task AddCommentParagraph(PageParser parser, ArticleParagraph paragraph)
        {
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.CreateCommentParagraph),
                paragraph);
            await parser.ParseParagraph(paragraph.Text, paragraph.ParagraphIndex);
            var citations = parser.Citations;
            var tags = parser.Tags;
            List<CrossReference> crossReferencesToAdd =
                await CitationConverter.ToCrossReferences(citations, paragraph.ID, paragraph.ParagraphIndex);
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.CreateCrossReferences),
                crossReferencesToAdd);
        }

        internal async static Task AddArticleParagraph(PageParser parser, ArticleParagraph paragraph)
        {
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.CreateArticleParagraph),
                paragraph);
            await parser.ParseParagraph(paragraph.Text, paragraph.ParagraphIndex);
            var citations = parser.Citations;
            var tags = parser.Tags;
            List<CrossReference> crossReferencesToAdd =
                await CitationConverter.ToCrossReferences(citations, paragraph.ID, paragraph.ParagraphIndex);
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.CreateRelatedArticleLinks),
                crossReferencesToAdd);
            await AddDefinitionSearches(paragraph, citations);
            List<int> relatedIDs = await GetRelatedIDs(parser.Tags);
            List<RelatedTag> tagsToAdd = new List<RelatedTag>();
            for (int index = Ordinals.first; index < relatedIDs.Count; index++)
            {
                tagsToAdd.Add(new RelatedTag(paragraph.ID, paragraph.ParagraphIndex, relatedIDs[index]));
            }
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.CreateRelatedTags),
                tagsToAdd);
        }

        private static async Task<List<Citation>> ReadCrossReferences(int ID, int paragraphIndex)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadCrossReferences),
               ID, paragraphIndex);
            var citationRanges = await reader.GetData<int, int>();
            reader.Close();
            List<Citation> citations = new List<Citation>();
            for (int index = Ordinals.first; index < citationRanges.Count; index++)
            {
                citations.Add(new Citation(citationRanges[index].Item1,
                    citationRanges[index].Item2) { CitationType = CitationTypes.Text });
            }
            return citations;
        }
        private static async Task<List<Citation>> ReadRelatedArticleLinks(int ID, int paragraphIndex)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadRelatedArticleLinks),
               ID, paragraphIndex);
            var citationRanges = await reader.GetData<int, int>();
            reader.Close();
            List<Citation> citations = new List<Citation>();
            for (int index = Ordinals.first; index < citationRanges.Count; index++)
            {
                citations.Add(new Citation(citationRanges[index].Item1,
                    citationRanges[index].Item2)
                { CitationType = CitationTypes.Text });
            }
            return citations;
        }

        public static (List<Citation> citationsToAdd, List<Citation> citationsToDelete) 
            CompareCitationLists(List<Citation> newCitations, List<Citation> oldCitations)
        {
            List<Citation> commonCitations = new List<Citation>();
            List<Citation> citationsToAdd = new List<Citation>();

            for (int index = Ordinals.first; index < newCitations.Count; index++)
            {
                bool found = false;
                for (int otherIndex = Ordinals.first; otherIndex < oldCitations.Count; otherIndex++)
                {
                    if (oldCitations[otherIndex].Equals(newCitations[index]))
                    {
                        commonCitations.Add(newCitations[index]);
                        found = true;
                        break;
                    }
                }
                if (!found) citationsToAdd.Add(newCitations[index]);
            }

            List<Citation> citationsToDelete = new List<Citation>();
            if (oldCitations.Count != commonCitations.Count)
            {
                for (int index = Ordinals.first; index < oldCitations.Count; index++)
                {
                    bool found = false;
                    for (int otherIndex = Ordinals.first; otherIndex < commonCitations.Count; otherIndex++)
                    {
                        if (oldCitations[index].Equals(commonCitations[otherIndex]))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) citationsToDelete.Add(oldCitations[index]);
                }
            }
            return (citationsToAdd, citationsToDelete);
        }

        public static async Task UpdateNavigationParagraph(MarkupParser parser, ArticleParagraph paragraph)
        {
            await DataWriterProvider.WriteData(SqlServerInfo.GetCommand(DataOperation.UpdateNavigationParagraph),
                paragraph);
            await parser.ParseParagraph(paragraph.Text, paragraph.ParagraphIndex);
        }
    }
}