using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Feliciana.ResponseWriter;
using Feliciana.Data;
using Myriad.Library;
using Myriad.Parser;
using Myriad.Data;
using Myriad.Search;
using Feliciana.HTML;
using Feliciana.Library;
using Myriad.Formatter;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Myriad.Pages
{
    public class VersePage : ScripturePage
    {
        public const string pageURL = "/Verse";
        public const string editURL = "/Verse/Edit";
        VersePageInfo info = new VersePageInfo();
        List<Keyword> keywords;

        public override string GetURL()
        {
            return pageURL;
        }

        //TODO research page

        protected override CitationTypes GetCitationType()
        {
            return CitationTypes.Verse;
        }

        protected override async Task WriteTitle(HTMLWriter writer)
        {
            await CitationConverter.ToLongString(citation, writer);
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

            citation.CitationRange.Set(citation.CitationRange.Book, citation.CitationRange.FirstChapter,
                citation.CitationRange.FirstVerse);
            await WriteRubyText(writer);
            await ArrangePhraseComments();
            await WritePhraseComments(writer, info);
            await WriteAdditionalComments(writer, info);
            await AddPageTitleData(writer);
            await AddPageHistory(writer);
            await AddEditPageData(writer);
        }

        private async Task WriteRubyText(HTMLWriter writer)
        {
            await StartRubySection(writer);
            keywords = ReadKeywords(citation);
            if (keywords != null)
            {
                int lastWordOnTop = -1;
                List<RubyInfo> wordsOnTop = new List<RubyInfo>();
                for (int i = Ordinals.first; i < keywords.Count; i++)
                {
                    if (lastWordOnTop == Result.notfound)
                    {
                        wordsOnTop = GetSustituteText(keywords[i].ID);
                        if (wordsOnTop.Count > Number.nothing)
                        {
                            await writer.Append("<ruby>");
                            lastWordOnTop = wordsOnTop[Ordinals.last].EndID;
                        }
                    }
                    await TextFormatter.AppendTextOfKeyword(writer, keywords[i]);
                    if (keywords[i].ID == lastWordOnTop)
                    {
                        await writer.Append("<rt>");
                        await WriteSubstituteText(writer, wordsOnTop);
                        await writer.Append("</rt></ruby>");
                        lastWordOnTop = Result.notfound;
                    }
                }
            }
            await EndRubySection(writer);
        }

        internal async Task UpdateMatrix(HTMLWriter writer, IQueryCollection query, string text)
        {
            string matrix = text.Replace('\'', '’');
            string[] words = matrix.Split(new string[] { "} {" }, StringSplitOptions.RemoveEmptyEntries);
            int start = Numbers.Convert(query[queryKeyStart]);
            int end = Numbers.Convert(query[queryKeyEnd]);
            int length = await CitationConverter.ReadLastWordIndex(start, end) + 1;
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
                words[Ordinals.last] = words[Ordinals.last][Ordinals.first..Ordinals.nexttolast];
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
            (int sentenceID, int sentenceWordIndex) = await ReadSentenceIndex(id);
            List<MatrixWord> oldInflections = ReadMatrixWords(id).OrderBy(i => i.Text).ToList();
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
            var reader = new DataReaderProvider<int, string>(SqlServerInfo.GetCommand(DataOperation.ReadDefinitionSearchID),
                matrixWord.Start, matrixWord.Text);
            int id = await reader.GetDatum<int>();
            reader.Close();
            if (id > Number.nothing) return;
            var relatedReader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadRelatedArticles),
                matrixWord.Start, matrixWord.End);
            var relatedArticles = relatedReader.GetClassData<RangeAndParagraph>();

            for (int index = Ordinals.first; index < relatedArticles.Count; index++)
            {
                List<string> synonyms = ArticlePage.GetSynonyms(relatedArticles[index].ArticleID);
                if (synonyms.Contains(matrixWord.Text))
                {
                    DefinitionSearch searchword = new DefinitionSearch(matrixWord, 
                        relatedArticles[index].ArticleID, relatedArticles[index].ParagraphIndex, sentenceID, wordIndex);
                    await DataWriterProvider.WriteDataObject(SqlServerInfo.GetCommand(DataOperation.CreateDefinitionSearch),
                        searchword);
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

        private static async Task<(int sentenceID, int sentenceWordIndex)> ReadSentenceIndex(int id)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadSentenceIndex),
                id);
            (int sentenceID, int wordIndex) = await reader.GetDatum<int, int>();
            reader.Close();
            return (sentenceID, wordIndex);
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

        private async Task WriteSubstituteText(HTMLWriter writer, List<RubyInfo> wordsOnTop)
        {
            for (int i = Ordinals.first; i < wordsOnTop.Count; i++)
            {
                if (i > Ordinals.first)
                    await writer.Append(Symbol.space);
                await writer.Append(wordsOnTop[i].Text.Replace('_', ' '));
            }
        }

        internal async Task WritePlainText(HTMLWriter writer, IQueryCollection query)
        {
            int start = Numbers.Convert(query[queryKeyStart]);
            int end = Numbers.Convert(query[queryKeyEnd]);
            end = start+await CitationConverter.ReadLastWordIndex(start, end);
            for (int id = start; id<= end; id++)
            {
                if (id > start) await writer.Append(' ');
                string words = GetMatrixWordsString(id);
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

        private string GetMatrixWordsString(int id)
        {
            List<MatrixWord> words = ReadMatrixWords(id);
            StringBuilder result = new StringBuilder();
            for (int index = Ordinals.first; index < words.Count; index++)
            {
                if (index > Ordinals.first) result.Append(' ');
                result.Append(words[index].ToString());
            }
            return result.ToString();
        }

        private static List<MatrixWord> ReadMatrixWords(int id)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadMatrixWords),
                id);
            var words = reader.GetClassData<MatrixWord>();
            reader.Close();
            return words;
        }

        private List<RubyInfo> GetSustituteText(int id)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadSubtituteWords),
                id);
            List<RubyInfo> substituteWords = reader.GetClassData<RubyInfo>();
            reader.Close();
            return substituteWords;
        }


        private async Task EndRubySection(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.EndParagraph +
                HTMLTags.EndDiv +
                HTMLTags.EndSection);
        }

        private async Task StartRubySection(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.StartSectionWithClass +
                HTMLClasses.scripture +
                HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartMainHeader);
            await WriteTitle(writer);
            await writer.Append(HTMLTags.EndMainHeader +
                HTMLTags.StartDivWithClass +
                HTMLClasses.textquote +
                Symbol.space +
                HTMLClasses.versetext +
                HTMLTags.CloseQuoteEndTag+
                HTMLTags.StartParagraph
                );
        }

        public List<Keyword> ReadKeywords(Citation citation)
        {
            var reader = new DataReaderProvider<int, int>(
                SqlServerInfo.GetCommand(DataOperation.ReadKeywords),
                citation.CitationRange.StartID.ID, citation.CitationRange.EndID.ID);
            var result = reader.GetClassData<Keyword>();
            reader.Close();
            return result;
        }

        private async Task ArrangePhraseComments()
        {
            await info.LoadInfo(citation.CitationRange);
        }

        private async Task WritePhraseComments(HTMLWriter writer, VersePageInfo info)
        {
            for (int index = Ordinals.first; index < info.Phrases.Count; index++)
            {
                if (info.PhraseHasNoComments(index))
                {
                    if (info.OriginalWords.Any()) await WriteOriginalWordLabel(writer, index, info);
                    continue;
                }
                await WritePhraseComment(writer, index, info);
            }
        }
        private async Task WriteOriginalWordLabel(HTMLWriter writer, int index, VersePageInfo info)
        {
            var phrase = info.Phrases[index];
            IEnumerable<(string, (int start, int end))> originalWordsInPhrase = from w in info.OriginalWords
                                                                 where w.Start >= phrase.Range.start &&
                                                                 w.End <= phrase.Range.end
                                                                 select (w.Text, w.Range);
            int count = originalWordsInPhrase.Count();
            if (count == 0) return;
            await writer.Append(HTMLTags.StartParagraphWithClass +
                HTMLClasses.comment +
                HTMLTags.CloseQuoteEndTag+
                HTMLTags.StartBold);

            if (info.Ellipses.ContainsKey(phrase.Start))
                await WriteRangeText(writer, phrase.Range, 
                    (info.Ellipses[phrase.Start].StartID.ID, info.Ellipses[phrase.Start].EndID.ID));
            else
                await WriteRangeText(writer, phrase.Range);
            await writer.Append(HTMLTags.EndBold+"("+HTMLTags.StartItalic);
            int i = Ordinals.first;
            bool needSpace = false;
            foreach ((string text, (int start, int end) range) in originalWordsInPhrase)
            {
                if (info.Ellipses.ContainsKey(phrase.Start))
                {
                    if ((range.start == info.Ellipses[phrase.Start].StartID.ID) &&
                        (range.end == info.Ellipses[phrase.Start].EndID.ID))
                    {
                        continue;
                    }
                }
                if (needSpace) await writer.Append(Symbol.space);
                await writer.Append(text.Replace('_', ' '));
                needSpace = true;
                i++;
            }
            await writer.Append(HTMLTags.EndItalic+")");
        }
        private async Task WritePhraseComment(HTMLWriter writer, int index, VersePageInfo info)
        {
            bool needFullLabel = true;
            int usedIndex = Result.notfound;
            if (info.OriginalWordComments.ContainsKey(index))
                needFullLabel = await WriteOriginalWordComments(writer, info, index);
            if (info.OriginalWordCrossReferences.ContainsKey(index))
                needFullLabel = await WriteOriginalWordCrossreferences(writer, info, index, needFullLabel);
            if (needFullLabel && info.PhraseArticles.ContainsKey(index))
                (needFullLabel, usedIndex) =
                        await WriteExactMatchPhraseArticle(writer, info, index, needFullLabel);
            await WritePhraseArticles(writer, info, index, needFullLabel, usedIndex);
        }

        private async Task<bool> WriteOriginalWordComments(HTMLWriter writer, VersePageInfo info, int index)
        {
            PageParser parser = new PageParser(writer);
            parser.HideDetails();
            parser.SetTargetRange(citation.CitationRange);
            bool needFullLabel = true;
            await WriteFullOriginalWordLabel(writer, info, index);
            await writer.Append(": ");
            for (int i = Ordinals.first; i<info.OriginalWordComments[index].Count; i++) 
            {
                parser.SetParagraphInfo(ParagraphType.Comment, info.OriginalWordComments[index][i].articleID);
                List<string> paragraphs = TextSectionFormatter.ReadParagraphs(
                    info.OriginalWordComments[index][i].articleID);
                parser.SetStartHTML("");
                parser.SetEndHTML(HTMLTags.EndParagraph);
                for (int paragraphIndex = Ordinals.first; paragraphIndex < paragraphs.Count; paragraphIndex++)
                {
                    if (paragraphIndex == Ordinals.second)
                        parser.SetStartHTML(HTMLTags.StartParagraphWithClass +
                            HTMLClasses.comment +
                            HTMLTags.CloseQuoteEndTag);
                    await parser.ParseParagraph(paragraphs[paragraphIndex], paragraphIndex);
                }
                needFullLabel = false;
            }
            return needFullLabel;
        }

        private async Task<bool> WriteOriginalWordCrossreferences(HTMLWriter writer, VersePageInfo info, int index, bool needFullLabel)
        {
            PageParser parser = new PageParser(writer);
            parser.SetTargetRange(citation.CitationRange);
            parser.HideDetails();
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadCommentParagraph),
                -1, -1);
            for (int i = Ordinals.first; i < info.OriginalWordCrossReferences[index].Count; i++) 
            {
                int articleID = info.OriginalWordCrossReferences[index][i].articleID;
                int paragraphIndex = info.OriginalWordCrossReferences[index][i].paragraphIndex;
                if (needFullLabel)
                {
                    await WriteFullCrossReferenceLabel(writer, info, index, articleID);
                    needFullLabel = false;
                }
                else
                {
                    await writer.Append(HTMLTags.StartParagraphWithClass +
                        HTMLClasses.comment +
                        HTMLTags.CloseQuoteEndTag);
                    await WriteVerseLabel(writer, articleID);
                }
                await writer.Append(": <span class=parcontent>");
                parser.SetParagraphInfo(ParagraphType.Comment, articleID);
                parser.SetStartHTML("");
                parser.SetEndHTML(HTMLTags.EndParagraph);
                reader.SetParameter(articleID, paragraphIndex);
                string paragraph = await reader.GetDatum<string>();
                await parser.ParseParagraph(paragraph, paragraphIndex);
            }
            reader.Close();
            return needFullLabel;
        }

        private async Task WriteFullCrossReferenceLabel(HTMLWriter writer, VersePageInfo info, int index, int commentID)
        {
            (int start, int end) phraseRange = info.Phrases[index].Range;

            var originalWordsInPhrase = (from w in info.OriginalWords
                                         where w.Start >= phraseRange.start &&
                                         w.End <= phraseRange.end
                                         select w.Text).ToList();
            int count = originalWordsInPhrase.Count();
            await writer.Append(HTMLTags.StartParagraphWithClass +
                HTMLClasses.comment +
                HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartBold);

            await WriteRangeText(writer, info.Phrases[index].Range);
            await writer.Append(HTMLTags.EndBold + "(");

            await WriteVerseLabel(writer, commentID);
            if (count > 0) await writer.Append("; " + HTMLTags.StartItalic);

            for (int i = Ordinals.first; i < originalWordsInPhrase.Count; i++)
            {
                if (i > Ordinals.first) await writer.Append(Symbol.space);
                await writer.Append(originalWordsInPhrase[i].Replace('_', ' '));
            }
            if (count > 0) await writer.Append(HTMLTags.EndItalic);
            await writer.Append(")");
        }

        private static async Task WriteVerseLabel(HTMLWriter writer, int commentID)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadCommentLinks),
                commentID);
            (int start, int end) range = await reader.GetDatum<int, int>();
            Citation crossreference = new Citation(range.start, range.end);
            crossreference.CitationType = (crossreference.CitationRange.Length < 10) ?
                 CitationTypes.Verse :
                 CitationTypes.Text;
            await CitationConverter.AppendLink(writer, crossreference);
        }

        private async Task WriteFullOriginalWordLabel(HTMLWriter writer, VersePageInfo info, int index)
        {
            (int start, int end) phraseRange = info.Phrases[index].Range;

            IEnumerable<(string, (int start, int end))> originalWordsInPhrase = from w in info.OriginalWords
                                                                 where w.Start >= phraseRange.start &&
                                                                 w.End <= phraseRange.end
                                                                 select (w.Text, w.Range);
            int count = originalWordsInPhrase.Count();
            await writer.Append(HTMLTags.StartParagraphWithClass +
                HTMLClasses.comment +
                HTMLTags.CloseQuoteEndTag+
                HTMLTags.StartBold);
            var phrase = info.Phrases[index];
            if (info.Ellipses.ContainsKey(phrase.Start))
                await WriteRangeText(writer, phrase.Range, info.Ellipses[phrase.Start].Range);
            else
                await WriteRangeText(writer, info.Phrases[index].Range);
            await writer.Append(HTMLTags.EndBold);
            if (count > 0) await writer.Append("("+HTMLTags.StartItalic);
            int i = Ordinals.first;
            bool needSpace = false;
            foreach ((string text, (int start, int end) range) in originalWordsInPhrase)
            {
                if (info.Ellipses.ContainsKey(phrase.Start))
                {
                    if ((range.start == info.Ellipses[phrase.Start].StartID.ID) &&
                        (range.end == info.Ellipses[phrase.Start].EndID.ID))
                    {
                        continue;
                    }
                }
                if (needSpace) await writer.Append(Symbol.space);
                await writer.Append(text.Replace("_", HTMLTags.NonbreakingSpace));
                needSpace = true;
                i++;
            }
            if (count > 0) await writer.Append(HTMLTags.EndItalic+")");
        }

        private async Task<(bool needFullLabel, int usedIndex)>
            WriteExactMatchPhraseArticle(HTMLWriter writer, VersePageInfo info, int index, bool needFullLabel)
        {
            (int start, int end) range = info.PhraseArticles[index].First().Value.range;
            if ((range.end - range.start) > 8) return (needFullLabel, Result.notfound);
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadArticleParagraph),
                -1, -1);
            int resultIndex = Result.notfound;
            string offsetLabel = RangeText(range).Trim();
            List<string> offsetRoots = Inflections.RootsOf(offsetLabel);
            string offsetRoot = ((offsetRoots.Count > Number.nothing) && (!string.IsNullOrEmpty(offsetRoots[Ordinals.first]))) ?
                offsetRoots[Ordinals.first] :
                offsetLabel;
            //Check for exact match between label and article titles
            PageParser parser = new PageParser(writer);
            parser.SetTargetRange(citation.CitationRange);
            parser.HideDetails();
            parser.SetStartHTML("");
            parser.SetEndHTML(HTMLTags.EndParagraph);
            for (int usedIndex=Ordinals.first; usedIndex< info.PhraseArticles[index].Count; usedIndex++)
            {
                string title = info.PhraseArticles[index].Keys.ElementAt(usedIndex);
                ((int start, int end) range,
                int articleID, List<int> paragraphIndices) entry = info.PhraseArticles[index][title];
                if ((title == offsetRoot ||
                    (title == offsetLabel)))
                {
                    await writer.Append(HTMLTags.StartParagraphWithClass +
                        HTMLClasses.comment +
                        HTMLTags.CloseQuoteEndTag);
                    await WriteFullLabel(writer, info, index, entry.range, entry.articleID, title);
                    bool first = true;

                    parser.SetParagraphInfo(ParagraphType.Article, entry.articleID);
                    foreach (int paragraphIndex in entry.paragraphIndices)
                    {
                        if (first) first = false;
                        else
                        {
                            await writer.Append(HTMLTags.StartParagraphWithClass +
                                HTMLClasses.comment +
                                HTMLTags.CloseQuoteEndTag);
                        }
                        reader.SetParameter(entry.articleID, paragraphIndex);
                        string paragraph = await reader.GetDatum<string>();
                        await parser.ParseParagraph(paragraph, paragraphIndex);
                    }
                    needFullLabel = false;
                    resultIndex = usedIndex;
                    break;
                }
            }

            if (resultIndex == Result.notfound)
            {
                for (int usedIndex = Ordinals.first; usedIndex < info.PhraseArticles[index].Count; usedIndex++)
                {
                    string title = info.PhraseArticles[index].Keys.ElementAt(usedIndex);
                    ((int start, int end) range,
                    int articleID, List<int> paragraphIndices) entry = info.PhraseArticles[index][title];
                    List<string> synonyms = ArticlePage.GetSynonyms(entry.articleID);
                    if (synonyms.Contains(offsetRoot) ||
                        synonyms.Contains(await AllWords.Conform(offsetLabel)))
                    {
                        await writer.Append(HTMLTags.StartParagraphWithClass +
                            HTMLClasses.comment +
                            HTMLTags.CloseQuoteEndTag);
                        await WriteFullLabel(writer, info, index, entry.range, entry.articleID, title);
                        bool first = true;
                        parser.SetParagraphInfo(ParagraphType.Article, entry.articleID);
                        foreach (int paragraphIndex in entry.paragraphIndices)
                        {
                            if (first) first = false;
                            else
                            {
                                await writer.Append(HTMLTags.StartParagraphWithClass +
                                    HTMLClasses.comment +
                                    HTMLTags.CloseQuoteEndTag);
                            }
                            reader.SetParameter(entry.articleID, paragraphIndex);
                            string paragraph = await reader.GetDatum<string>();
                            await parser.ParseParagraph(paragraph, paragraphIndex);
                        }
                        needFullLabel = false;
                        resultIndex = usedIndex;
                        break;
                    }
                }
            }
            reader.Close();
            return (needFullLabel, resultIndex);
        }
        private async Task WriteFullLabel(HTMLWriter writer, VersePageInfo info, int index, (int start, int end) wordRange, int articleID, string articleTitle)
        {
            (int start, int end) phraseRange = info.Phrases[index].Range;
            string label = RangeText(phraseRange);
            label = label.Trim();
            List<string> synonyms = ArticlePage.GetSynonyms(articleID);
            List<string> roots = Inflections.RootsOf(label);
            string root = ((roots.Count > Number.nothing) && (!string.IsNullOrEmpty(roots[Ordinals.first]))) ?
                roots[Ordinals.first] :
                label;
            string searchRoot = SearchPhrase(phraseRange);
            bool substitute = !(synonyms.Contains(root) || synonyms.Contains(searchRoot) || 
                synonyms.Contains(await AllWords.Conform(label)));
            bool part = false;

            if (substitute)
            {
                if (!wordRange.Equals(phraseRange))
                {
                    string offsetLabel = SearchPhrase(wordRange).Replace('_', ' ');
                    if ((offsetLabel.Length > 1) && (offsetLabel.Last() == ' '))
                        offsetLabel = offsetLabel.Remove(offsetLabel.Length - 1);
                    List<string> offsetRoots = Inflections.RootsOf(offsetLabel);
                    string offsetRoot = ((offsetRoots.Count > Number.nothing) &&
                        (!string.IsNullOrEmpty(offsetRoots[Ordinals.first]))) ?
                        offsetRoots[Ordinals.first].Replace('_', ' ') :
                        offsetLabel.Replace('_', ' ');
                    if (synonyms.Contains(offsetRoot) ||
                        synonyms.Contains(await AllWords.Conform(offsetLabel)))
                    {
                        int i = label.IndexOf(offsetLabel);
                        int j = i + offsetLabel.Length;
                        substitute = false;
                        part = true;
                        if (i == Result.notfound)
                        {
                            i = Ordinals.first;
                            j = label.Length;
                        }
                        await writer.Append(HTMLTags.StartBold);
                        await writer.Append(label.Substring(Ordinals.first, i));
                        await WriteTagAnchor(writer, offsetLabel, articleID);
                        if (j < label.Length - 1) await writer.Append(label.Substring(j));
                    }
                }
            }

            if (substitute)
            {
                await writer.Append(HTMLTags.StartBold);
                await writer.Append(label);
            }
            else
                if (!part)
            {
                await writer.Append(HTMLTags.StartBold);
                await WriteTagAnchor(writer, label, articleID);
            }
            label = info.OriginalWordsInRange(phraseRange.start, phraseRange.end);
            if (string.IsNullOrEmpty(label))
            {
                if (substitute)
                {
                    await writer.Append(HTMLTags.EndBold+" (");
                    await WriteTagAnchor(writer, articleTitle, articleID);
                    await writer.Append("): ");
                }
                else
                    await writer.Append(":"+HTMLTags.EndBold+Symbol.space);
            }
            else
            {
                if (substitute)
                {
                    string cleanLabel = Inflections.RemoveDiacritics(label);
                    if (synonyms.Contains(cleanLabel))
                    {
                        await writer.Append(HTMLTags.EndBold +
                            " (" + HTMLTags.StartItalic);
                        await WriteTagAnchor(writer, label, articleID);
                        await writer.Append(HTMLTags.EndItalic+"): ");
                    }
                    else
                    {
                        await writer.Append(HTMLTags.EndBold+" (");
                        await WriteTagAnchor(writer,
                            articleTitle.Replace('(', '[').Replace(')', ']'),
                            articleID);
                        await writer.Append("; "+HTMLTags.StartItalic);
                        await writer.Append(label);
                        await writer.Append(HTMLTags.EndItalic+"): ");
                    }
                }
                else
                {
                    await writer.Append(HTMLTags.EndBold+
                        " ("+HTMLTags.StartItalic);
                    await writer.Append(label);
                    await writer.Append(HTMLTags.EndItalic+"): ");
                }
            }
        }

        private string SearchPhrase((int start, int end) wordRange)
        {
            var reader = new DataReaderProvider<int>(SqlServerInfo.GetCommand(DataOperation.ReadSearchPhrase),
                wordRange.start);
            List<string> words = reader.GetData<string>();
            reader.Close();
            StringBuilder result = new StringBuilder();
            for (int i = Ordinals.first; i < words.Count; i++)
            {
                if (i > Ordinals.first) result.Append(' ');
                result.Append(words[i]);
            }
            return result.ToString();
        }

        private async Task WriteTagAnchor(HTMLWriter writer, string label, int articleID)
        {
            await PageFormatter.WriteTagAnchor(writer, label, articleID, citation.CitationRange);
        }
        private async Task WritePhraseArticles(HTMLWriter writer, VersePageInfo info, int index, bool needFullLabel, int usedIndex)
        {
            int currentIndex = Ordinals.first;
            if (info.PhraseArticles.ContainsKey(index))
            {
                PageParser parser = new PageParser(writer);
                parser.HideDetails();
                parser.SetTargetRange(citation.CitationRange);
                parser.SetStartHTML("");
                parser.SetEndHTML(HTMLTags.EndParagraph);
                var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadArticleParagraph),
                    -1, -1);
                for (int i = Ordinals.first; i < info.PhraseArticles[index].Count; i++)
                {
                    string title = info.PhraseArticles[index].Keys.ElementAt(i);
                    ((int start, int end) range,
                    int articleID, List<int> paragraphIndices) entry = info.PhraseArticles[index][title];
                    if (currentIndex == usedIndex)
                    {
                        currentIndex++;
                        continue;
                    }
                    bool needLabel = true;
                    if (needFullLabel)
                    {
                        await writer.Append(HTMLTags.StartParagraphWithClass +
                            HTMLClasses.comment +
                            HTMLTags.CloseQuoteEndTag);
                        await WriteFullLabel(writer, info, index, 
                            entry.range, entry.articleID, title);
                        needFullLabel = false;
                        needLabel = false;
                    }
                    bool first = true;
                    parser.SetParagraphInfo(ParagraphType.Article, entry.articleID);
                    foreach (int paragraphIndex in entry.paragraphIndices)
                    {
                        if (first) first = false;
                        else
                        {
                            await writer.Append(HTMLTags.StartParagraphWithClass +
                                HTMLClasses.comment +
                                HTMLTags.CloseQuoteEndTag);
                        }
                        if (needLabel)
                        {
                            await writer.Append(HTMLTags.StartParagraphWithClass +
                                HTMLClasses.comment +
                                HTMLTags.CloseQuoteEndTag);
                            await WriteTagAnchor(writer, title, entry.articleID);
                            await writer.Append(": ");
                            needLabel = false;
                        }
                        reader.SetParameter(entry.articleID, paragraphIndex);
                        string paragraph = await reader.GetDatum<string>();
                        await parser.ParseParagraph(paragraph, paragraphIndex);
                    }
                    currentIndex++;
                }
                reader.Close();
            }
        }

        private async Task WriteAdditionalComments(HTMLWriter writer, VersePageInfo info)
        {
            int lastID = -1;
            bool first = true;
            PageParser parser = new PageParser(writer);
            parser.HideDetails();
            parser.SetTargetRange(citation.CitationRange);
            parser.SetStartHTML("");
            parser.SetEndHTML(HTMLTags.EndParagraph);
            var articleReader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadArticleParagraph),
                -1, -1);
            var commentReader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadCommentParagraph),
                -1, -1);
            for (int i = Ordinals.first; i<info.AdditionalArticles.Count; i++)
            {
                string title = info.AdditionalArticles.Keys.ElementAt(i);
                List<(int articleID, int paragraphIndex, bool suppressed)> entry = info.AdditionalArticles[title];
                if (first)
                {
                    await writer.Append(HTMLTags.StartHeader+
                        "Additional References"+
                        HTMLTags.EndHeader);
                    first = false;
                }
                int id = info.AdditionalArticleIDs[title];
                if (id != lastID)
                {
                    await AppendArticleTitleLabel(writer, id, title, entry[Ordinals.first].suppressed);
                    lastID = id;
                }
                else
                {
                    await StartParagraph(writer, entry[Ordinals.first].suppressed);
                }
                for (int j = Ordinals.first; j<entry.Count; j++)
                {
                    parser.SetParagraphInfo(ParagraphType.Article, entry[j].articleID);
                    if (j>Ordinals.first) 
                    {
                        await StartParagraph(writer, entry[j].suppressed);
                    }
                    articleReader.SetParameter(entry[j].articleID, entry[j].paragraphIndex);
                    string paragraph = await articleReader.GetDatum<string>();
                    await parser.ParseParagraph(paragraph, entry[j].paragraphIndex);
                }
            }
            (int start, int end) lastRange = (-1, -1);
            for (int i = Ordinals.first; i<info.AdditionalCrossReferences.Count; i++)
            {
                (int start, int end) range = info.AdditionalCrossReferences.Keys.ElementAt(i);
                List<(int articleID, int paragraphIndex, bool suppressed)> entry = info.AdditionalCrossReferences[range];
                if (first)
                {
                    await writer.Append(HTMLTags.StartHeader +
                        "Additional References" +
                        HTMLTags.EndHeader);
                    first = false;
                }
                if (!range.Equals(lastRange))
                {
                    await StartParagraph(writer, entry[Ordinals.first].suppressed);
                    Citation citation = new Citation(range.start, range.end);
                    citation.CitationType = CitationTypes.Text;
                    await CitationConverter.AppendLink(writer, citation);
                    await writer.Append(": ");
                    lastRange = range;
                }
                else
                {
                    await StartParagraph(writer, entry[Ordinals.first].suppressed);
                }
                bool firstParagraph = true;
                for (int j=Ordinals.first; j<entry.Count; j++)
                {
                    (int articleID, int paragraphIndex, bool suppressed) = entry[j];
                    if (firstParagraph) firstParagraph = false;
                    else
                    {
                        await StartParagraph(writer, suppressed);
                    }
                    parser.SetParagraphInfo(ParagraphType.Comment, articleID);
                    commentReader.SetParameter(articleID, paragraphIndex);
                    string paragraph = await commentReader.GetDatum<string>();
                    await parser.ParseParagraph(paragraph, paragraphIndex);
                }
            }
            articleReader.Close();
            commentReader.Close();
        }
        private async Task StartParagraph(HTMLWriter writer, bool suppressed)
        {
            await writer.Append(HTMLTags.StartParagraphWithClass +
                HTMLClasses.comment);
            if (suppressed)
            {
                await writer.Append(Symbol.space + HTMLClasses.suppressed);
            }
            await writer.Append(HTMLTags.CloseQuoteEndTag);
        }
        private async Task AppendArticleTitleLabel(HTMLWriter writer, int articleID, string title, bool suppressed)
        {
            await StartParagraph(writer, suppressed);
            await WriteTagAnchor(writer, title, articleID);
            await writer.Append(": ");
        }
        public override Task SetupNextPage()
        {
            int v = citation.CitationRange.FirstVerse + 1;
            int c = citation.CitationRange.FirstChapter;
            int b = citation.CitationRange.Book;
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
            int v = citation.CitationRange.FirstVerse - 1;
            int c = citation.CitationRange.FirstChapter;
            int b = citation.CitationRange.Book;
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

        public override async Task AddTOC(HTMLWriter writer)
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
        private async Task WriteRangeText(HTMLWriter writer, (int start, int end) enclosingRange, (int start, int end) ellipsisRange)
        {
            var wordsInRange = from w in keywords
                               where w.ID >= enclosingRange.start &&
                               w.ID <= enclosingRange.end
                               select w;

            bool first = true;
            bool opened = false;
            bool ellipsis = false;
            foreach (var word in wordsInRange)
            {
                if ((word.ID >= ellipsisRange.start) && (word.ID <= ellipsisRange.end))
                {
                    if (!ellipsis) await writer.Append("... ");
                    ellipsis = true;
                    continue;
                }
                ellipsis = false;
                if ((!first) && (HasOpenBracket(word))) opened = true;
                if (!opened) await writer.Append(TextWithoutSymbols(word));
                else
                    await writer.Append(FormattedTextWithBrackets(word));
                if (HasCloseBracket(word)) opened = false;
                first = false;
            }
            if (opened) await writer.Append(']');
        }
        private async Task WriteRangeText(HTMLWriter writer, (int start, int end) range)
        {
            var wordsInRange = from w in keywords
                               where w.ID >= range.start &&
                               w.ID <= range.end
                               select w;

            bool first = true;
            bool opened = false;
            foreach (var word in wordsInRange)
            {
                if ((!first) && (HasOpenBracket(word))) opened = true;
                if (!opened) await writer.Append(TextWithoutSymbols(word));
                else
                    await writer.Append(FormattedTextWithBrackets(word));
                if (HasCloseBracket(word)) opened = false;
                first = false;
            }
            if (opened) await writer.Append(']');
        }

        private string RangeText((int start, int end) range)
        {
            StringBuilder result = new StringBuilder();
            var wordsInRange = from w in keywords
                               where w.ID >= range.start &&
                               w.ID <= range.end
                               select w;

            bool first = true;
            bool opened = false;
            foreach (var word in wordsInRange)
            {
                if ((!first) && (HasOpenBracket(word))) opened = true;
                if (!opened) result.Append(TextWithoutSymbols(word));
                else
                    result.Append(FormattedTextWithBrackets(word));
                if (HasCloseBracket(word)) opened = false;
                first = false;
            }
            if (opened) result.Append(']');
            return result.ToString();
        }

        private bool HasOpenBracket(Keyword word)
        {
            return word.LeadingSymbols.Contains('[');
        }
        private bool HasCloseBracket(Keyword word)
        {
            return word.TrailingSymbols.Contains(']');
        }

        internal string TextWithoutSymbols(Keyword word)
        {
            StringBuilder result = new StringBuilder();
            result.Append(Capitalize(word).Replace('`', '’'));
            result.Append(' ');
            result.Replace("<br>", "");
            return result.ToString();
        }

        private string Capitalize(Keyword word)
        {
            return (word.IsCapitalized) ?
                Symbols.Capitalize(word.TextString) :
                word.TextString;
        }
        internal string FormattedTextWithBrackets(Keyword word)
        {
            StringBuilder result = new StringBuilder();
            if (word.LeadingSymbols.IndexOf('[') != Result.notfound) result.Append('[');
            result.Append(Capitalize(word).Replace('`', '’'));
            if (word.TrailingSymbols.IndexOf(']') != Result.notfound) result.Append(']');
            result.Append(' ');
            result.Replace("<br>", "");
            return result.ToString();
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
            await writer.Append(citation.CitationRange.StartID.ID);
            await writer.Append(HTMLTags.Ampersand +
                queryKeyEnd +
                Symbol.equal);
            await writer.Append(citation.CitationRange.EndID.ID);
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
