using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Feliciana.ResponseWriter;
using Myriad.Library;
using Myriad.Parser;
using Myriad.Data;
using Myriad.Search;
using Feliciana.HTML;
using Feliciana.Library;
using Myriad.Formatter;
using Microsoft.CodeAnalysis;

namespace Myriad.Pages
{
    public class VersePage : ScripturePage
    {
        public const string pageURL = "/Verse";
        public const string editURL = "/Edit/Verse";
        internal VersePageInfo info = new VersePageInfo();

        public override string GetURL()
        {
            return pageURL;
        }

        protected override CitationTypes GetCitationType()
        {
            return CitationTypes.Verse;
        }

        internal override async Task WriteTitle(HTMLWriter writer)
        {
            citation.LabelType = LabelTypes.Normal;
            await CitationConverter.ToString(citation, writer);
        }

        protected override string PageScripts()
        {
            return Scripts.Text;
        }

        public async override Task RenderBody(HTMLWriter writer)
        {
            if (citation.CitationType == CitationTypes.Text)
            {
                TextPage textPage = new TextPage();
                textPage.SetCitation(citation);
                textPage.SetResponse(response);
                await textPage.RenderBody(writer);
                return;
            }

            citation = new Citation(citation.Book, citation.FirstChapter,
                citation.FirstVerse);
            await VerseFormatter.Format(this, writer);
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
            await AddEditPageData(writer);
            await AddPagination(writer);
        }

        internal async Task UpdateOriginalWordComments(HTMLWriter writer, string text)
        {
            var originalWords = await DataRepository.OriginalWords(citation.Start, citation.End);
            var plainTextWriter = Writer.New();
            await WriteOriginalWordCommentPlainText(plainTextWriter, originalWords);
            string originalText = plainTextWriter.Response();
            string[] originalParagraphs = originalText.Split(Symbols.linefeedArray, StringSplitOptions.RemoveEmptyEntries);
            string[] newParagraphs = text.Split(Symbols.linefeedArray, StringSplitOptions.RemoveEmptyEntries);
            if (originalParagraphs.Length == newParagraphs.Length)
                await WriteOriginalWordCommentsToDatabase(originalWords, originalParagraphs, newParagraphs);
            await RenderBody(writer);
        }

        private async Task WriteOriginalWordCommentsToDatabase(List<(string text, int start, int end)> originalWords, 
            string[] originalParagraphs, string[] newParagraphs)
        {
            MarkupParser parser = new MarkupParser(Writer.New());
            for (int index = Ordinals.first; index < originalWords.Count; index++)
            {
                if (originalParagraphs[index] == newParagraphs[index]) continue;
                int p = originalParagraphs[index].IndexOf("//): ");
                int q = newParagraphs[index].IndexOf("//): ");
                if (p != q) continue;
                string originalComment = (originalParagraphs[index].Length > p + 5) ?
                    originalParagraphs[index][(p + 5)..] : "";
                string newComment = (newParagraphs[index].Length > q + 5) ?
                    newParagraphs[index][(q + 5)..] : "";
                if (originalComment == newComment) continue;
                if (originalComment == "")
                {
                    int newID = await DataRepository.NewOriginalWordCommentID();
                    parser.SetParagraphInfo(ParagraphType.Comment, newID);
                    ArticleParagraph newParagraph = new ArticleParagraph(newID, Ordinals.first, newComment);
                    newParagraph.OriginalWord = 1;
                    await DataRepository.WriteOriginalWordComment((originalWords[index].start, originalWords[index].end),
                        newID, newComment);
                    continue;
                }
                int id = await DataRepository.OriginalWordCommentID(originalWords[index].start, originalWords[index].end);
                await DataRepository.DeleteOriginalWordComment(id);
                if (newComment == "")
                {
                    continue;
                }
                await DataRepository.WriteOriginalWordComment((originalWords[index].start, originalWords[index].end),
                    id, newComment);
            }
        }

        internal async Task WriteOriginalWordComments(HTMLWriter writer)
        {
            List<(string text, int start, int end)> originalWords = await DataRepository.OriginalWords(citation.Start, citation.End);
            await WriteOriginalWordCommentPlainText(writer, originalWords);
        }

        private static async Task WriteOriginalWordCommentPlainText(HTMLWriter writer, List<(string text, int start, int end)> originalWords)
        {
            for (int index = Ordinals.first; index < originalWords.Count; index++)
            {
                List<(string text, int capitalized)> keywords = await DataRepository.KeywordsPlainText(originalWords[index].start, originalWords[index].end);
                await writer.Append("**");
                for (int keywordIndex = Ordinals.first; keywordIndex < keywords.Count; keywordIndex++)
                {
                    if (keywordIndex > Ordinals.first) await writer.Append(" ");
                    await writer.Append((keywords[keywordIndex].capitalized == 1) ?
                        Symbols.Capitalize(keywords[keywordIndex].text) :
                        keywords[keywordIndex].text);
                }
                await writer.Append("** (//");
                await writer.Append(originalWords[index].text);
                await writer.Append("//): ");
                string comment = await DataRepository.OriginalWordComment(originalWords[index].start, originalWords[index].end);
                if (comment != null) await writer.Append(comment);
                await writer.Append(Symbol.lineFeed);
            }
        }

        internal async Task UpdateMatrix(HTMLWriter writer, IQueryCollection query, string text)
        {
            string matrix = text.Replace('\'', '’');
            string[] words = matrix.Split(new string[] { "} {" }, StringSplitOptions.RemoveEmptyEntries);
            int start = Numbers.Convert(query[queryKeyStart]);
            int end = Numbers.Convert(query[queryKeyEnd]);
            KeyID keyID = new KeyID(end);
            int length = await DataRepository.LastWordIndex(keyID.ID) + 1;
            end = start + length - 1;
            citation = new Citation(start, end);
            citation.CitationType = CitationTypes.Verse;
            if (words.Length != length)
            {
                await RenderBody(writer);
                return;
            }
            if (words[Ordinals.first][Ordinals.first] == '{') 
                words[Ordinals.first] = words[Ordinals.first][Ordinals.second..];
            if (words[Ordinals.last][Ordinals.last] == '}') 
                words[Ordinals.last] = words[Ordinals.last].Substring(Ordinals.first, words[Ordinals.last].Length-1);
            for (int id = start; id <= end; id++)
            {
                int index = id - start;
                List<MatrixWord> newInflections = await DecodeMatrixString(words[index], id);
                newInflections = newInflections.OrderBy(i => i.Text).ToList();
                await UpdateInflections(newInflections, id);
            }
            await RenderBody(writer);
        }

        private static async Task UpdateInflections(List<MatrixWord> newInflections, int id)
        {
            (int sentenceID, int sentenceWordIndex) = await DataRepository.SentenceWordIndex(id);
            List<MatrixWord> oldInflections = await DataRepository.MatrixWords(id);
            int existingIndex = Ordinals.first;
            int index = Ordinals.first;
            while (index < newInflections.Count)
            {
                if (existingIndex >= oldInflections.Count)
                {
                    await AddDefinitionSearch(newInflections[index], sentenceID, sentenceWordIndex);
                    await AddMatrixWord(newInflections[index], sentenceID, sentenceWordIndex);
                    index++;
                    continue;
                }
                if (newInflections[index].Text == oldInflections[existingIndex].Text)
                {
                    if ((newInflections[index].Weight != oldInflections[existingIndex].Weight) ||
                        (newInflections[index].Length != oldInflections[existingIndex].Length) ||
                        (newInflections[index].Substitute != oldInflections[existingIndex].Substitute))
                    {
                        await UpdateMatrixWord(oldInflections[existingIndex].Text, newInflections[index], 
                            sentenceID, sentenceWordIndex);
                    }
                    existingIndex++;
                    index++;
                    continue;
                }
                if (String.Compare(oldInflections[existingIndex].Text, newInflections[index].Text)>0)
                {
                    await AddDefinitionSearch(newInflections[index], sentenceID, sentenceWordIndex);
                    await AddMatrixWord(newInflections[index], sentenceID, sentenceWordIndex);
                    index++;
                    continue;
                }
                await DeleteDefinitionSearches(oldInflections[existingIndex]);
                await DeleteMatrixWord(oldInflections[existingIndex].Text, sentenceID, sentenceWordIndex);
                existingIndex++;
            }
            while (existingIndex < oldInflections.Count)
            {
                await DeleteDefinitionSearches(oldInflections[existingIndex]);
                await DeleteMatrixWord(oldInflections[existingIndex].Text, sentenceID, sentenceWordIndex);
                existingIndex++;
            }
        }

        private static async Task AddDefinitionSearch(MatrixWord matrixWord, int sentenceID, int wordIndex)
        {
            bool wordExists = await DataRepository.SearchWordExists(matrixWord.Start, matrixWord.Text);
            if (wordExists) return;
            var relatedArticles = await DataRepository.RelatedArticles(matrixWord.Start, matrixWord.End);
            for (int index = Ordinals.first; index < relatedArticles.Count; index++)
            {
                List<string> synonyms = await DataRepository.Synonyms(relatedArticles[index].id);
                if (synonyms.Contains(matrixWord.Text))
                {
                    int synIndex = await DataRepository.SynIndex(relatedArticles[index].id, matrixWord.Text);
                    DefinitionSearch searchword = new DefinitionSearch(matrixWord, 
                        relatedArticles[index].id, relatedArticles[index].paragraphindex, sentenceID, wordIndex, synIndex);
                    await DataRepository.WriteDefinitionSearch(searchword);
                }
            }
        }

        private static async Task DeleteDefinitionSearches(MatrixWord matrixWord)
        {
            var reader = new DataReaderProvider<int, string>(SqlServerInfo.GetCommand(DataOperation.ReadDefinitionSearchID),
                matrixWord.Start, matrixWord.Text);
            
            int id = await reader.GetDatum<int>();
            reader.Close();
            if (id == Number.nothing) return;
            await DataWriterProvider.Write(SqlServerInfo.GetCommand(DataOperation.DeleteDefinitionSearch), id);
        }

        private static async Task DeleteMatrixWord(string text, int sentenceID, int wordIndex)
        {
            await DataWriterProvider.Write(SqlServerInfo.GetCommand(DataOperation.DeleteMatrixWord),
                sentenceID, wordIndex, text);
        }

        private static async Task UpdateMatrixWord(string originalWord, MatrixWord newMatrixWord, int sentenceID, int wordIndex)
        {
            var reader = new DataReaderProvider<int, int, string>(
                SqlServerInfo.GetCommand(DataOperation.ReadSearchWordID),
                sentenceID, wordIndex, originalWord);
            int id = await reader.GetDatum<int>();
            reader.Close();
            await DataWriterProvider.Write<int, int, int, int>(
                SqlServerInfo.GetCommand(DataOperation.UpdateMatrixWord),
                id, newMatrixWord.Weight, newMatrixWord.End, (newMatrixWord.Substitute) ? 1 : 0);
        }

        private static async Task AddMatrixWord(MatrixWord matrixWord, int sentenceID, int wordIndex)
        {
            SearchResult searchword = new SearchResult(matrixWord, sentenceID, wordIndex);
            await DataWriterProvider.WriteDataObject(SqlServerInfo.GetCommand(DataOperation.CreateMatrixWord),
                searchword);
        }
        private async Task<List<MatrixWord>> DecodeMatrixString(string matrixString, int id)
        {
            string[] inflections = matrixString.Split(Symbols.spaceArray);
            var wordList = new List<string>();
            var inflectionList = new List<MatrixWord>();
            for (int index = Ordinals.first; index < inflections.Length; index++)
            {
                if (string.IsNullOrEmpty(inflections[index]) || (inflections[index] == "-")) continue;
                var inflection = new MatrixWord(inflections[index], id);
                if (wordList.Contains(inflection.Text)) continue;
                int p = inflection.Text.IndexOf('_');
                if (p > Result.notfound)
                {
                    string firstWord = inflection.Text[p..];
                    await Phrases.Add(firstWord, inflection.Text.Replace('_', ' '));
                }
                inflectionList.Add(inflection);
                wordList.Add(inflection.Text);
            }
            return inflectionList;
        }

        internal async Task WritePlainText(HTMLWriter writer, IQueryCollection query)
        {
            int start = Numbers.Convert(query[queryKeyStart]);
            int end = Numbers.Convert(query[queryKeyEnd]);
            KeyID keyID = new KeyID(end);
            end = start+ await DataRepository.LastWordIndex(keyID.ID);
            for (int id = start; id<= end; id++)
            {
                if (id > start) await writer.Append(' ');
                string words = await GetMatrixWordsString(id);
                if (words.Length > Number.nothing)
                {
                    await writer.Append('{');
                    await writer.Append(words);
                    await writer.Append("}");
                }
                else
                    await writer.Append("{-}");
            }
        }

        private async Task<string> GetMatrixWordsString(int id)
        {
            List<MatrixWord> words = await DataRepository.MatrixWords(id);
            StringBuilder result = new StringBuilder();
            for (int index = Ordinals.first; index < words.Count; index++)
            {
                if (index > Ordinals.first) result.Append(' ');
                result.Append(words[index].ToString());
            }
            return result.ToString();
        }


        public override Task SetupNextPage()
        {
            int v = citation.FirstVerse + 1;
            int c = citation.FirstChapter;
            int b = citation.Book;
            if (v > Bible.Chapters[b][c])
            {
                c++;
                v = 1;
            }
            if (c > Bible.Chapters[b].Length-1)
            {
                b++;
                if (b > 65)
                {
                    b = 65;
                    c = 22;
                    v = Bible.Chapters[b][c];
                }
                else
                    c = 1;
            }
            citation = new Citation(new KeyID(b, c, v), new KeyID(b, c, v, KeyID.MaxWordIndex));
            return Task.CompletedTask;
        }

        public override Task SetupPrecedingPage()
        {
            int v = citation.FirstVerse - 1;
            int c = citation.FirstChapter;
            int b = citation.Book;
            if (v < 1) c--;
            if (c < 1)
            {
                b--;
                if (b < 0)
                {
                    b = 0;
                    c = 1;
                    v = 1;
                }
            }
            if (c < 1) c = Bible.LastChapterInBook(b);
            if (v < 1) v = Bible.Chapters[b][c];
            citation = new Citation(new KeyID(b, c, v), new KeyID(b, c, v, KeyID.MaxWordIndex));
            return Task.CompletedTask;
        }

        public override async Task WriteTOC(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartList +
                HTMLTags.StartListItem +
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

        public override Task SetupParentPage()
        {
            citation.CitationType = CitationTypes.Text;
            return Task.CompletedTask;
        }


        private async Task AddEditPageData(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartDivWithID +
                HTMLClasses.editdata + HTMLTags.CloseQuote +
                HTMLTags.Class +
                HTMLClasses.hidden +
                HTMLTags.CloseQuoteEndTag +
                editURL + HTMLTags.StartQuery+
                queryKeyStart +
                Symbol.equal);
            await writer.Append(citation.Start);
            await writer.Append(HTMLTags.Ampersand +
                queryKeyEnd +
                Symbol.equal);
            await writer.Append(citation.End);
            await writer.Append(HTMLTags.EndDiv);
        }

        public override async Task HandleEditRequest(HttpContext context)
        {
            await WritePlainText(Writer.New(context.Response),
                context.Request.Query);
        }

        public override async Task HandleAcceptedEdit(HttpContext context)
        {
            context.Request.Form.TryGetValue("text", out var text);
            await UpdateMatrix(Writer.New(context.Response),
                context.Request.Query, text.ToString());
        }
    }
}
