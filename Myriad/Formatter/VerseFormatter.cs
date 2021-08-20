using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.ResponseWriter;
using Feliciana.HTML;
using Feliciana.Data;
using Myriad.Data;
using Myriad.Library;
using Myriad.Parser;
using Myriad.Pages;
using Microsoft.CodeAnalysis.Operations;

namespace Myriad.Formatter
{
    public static class VerseFormatter
    {
        public static async Task Format(VersePage page, HTMLWriter writer)
        {
            await WriteRubyText(writer, page);
            await ArrangePhraseComments(page);
            await WritePhraseComments(writer, page);
            await WriteAdditionalComments(writer, page);
        }
        private static async Task ArrangePhraseComments(VersePage page)
        {
            await page.info.LoadInfo(page.citation);
        }

        private static async Task WriteRubyText(HTMLWriter writer, VersePage page)
        {
            await StartRubySection(writer, page);
            page.info.keywords = await DataRepository.RangeKeywords(page.citation.Start, 
                page.citation.End);
            if (page.info.keywords != null)
            {
                int lastWordOnTop = -1;
                var wordsOnTop = new List<(string text, int last)>();
                for (int i = Ordinals.first; i < page.info.keywords.Count; i++)
                {
                    await writer.Append(page.info.keywords[i].LeadingSymbols);
                    if (lastWordOnTop == Result.notfound)
                    {
                        wordsOnTop = await DataRepository.SubstituteWords(page.info.keywords[i].ID);
                        if (wordsOnTop.Count > Number.nothing)
                        {
                            await writer.Append("<ruby>");
                            lastWordOnTop = wordsOnTop[Ordinals.last].last;
                        }
                    }
                    await writer.Append(page.info.keywords[i].CapitalizedText.Replace('`', '’'));
                    if (page.info.keywords[i].ID == lastWordOnTop)
                    {
                        await writer.Append("<rt>");
                        await WriteSubstituteText(writer, wordsOnTop);
                        await writer.Append("</rt></ruby>");
                        lastWordOnTop = Result.notfound;
                    }
                    await writer.Append(page.info.keywords[i].TrailingSymbols.Replace("<br>", "").Replace("— ", "—"));
                }
            }
            await EndRubySection(writer);
        }

        private static async Task WriteSubstituteText(HTMLWriter writer, List<(string text, int last)> wordsOnTop)
        {
            for (int i = Ordinals.first; i < wordsOnTop.Count; i++)
            {
                if (i > Ordinals.first)
                    await writer.Append(Symbol.space);
                await writer.Append(wordsOnTop[i].text.Replace('_', ' '));
            }
        }
        private static async Task EndRubySection(HTMLWriter writer)
        {
            await writer.Append(HTMLTags.EndParagraph +
                HTMLTags.EndDiv +
                HTMLTags.EndSection);
        }
        private static async Task WritePhraseComments(HTMLWriter writer, VersePage page)
        {
            for (int index = Ordinals.first; index < page.info.Phrases.Count; index++)
            {
                if (page.info.PhraseHasNoComments(index))
                {
                    if (page.info.OriginalWords.Any()) await WriteOriginalWordLabel(writer, index, page.info);
                    continue;
                }
                await WritePhraseComment(writer, index, page);
            }
        }

        private static async Task WriteAdditionalComments(HTMLWriter writer, VersePage page)
        {
            int lastID = -1;
            bool first = true;
            PageParser parser = new PageParser(writer);
            parser.HideDetails();
            parser.SetTargetRange(page.citation);
            parser.SetStartHTML("");
            parser.SetEndHTML(HTMLTags.EndParagraph);
         
            for (int i = Ordinals.first; i < page.info.AdditionalArticles.Count; i++)
            {
                string title = page.info.AdditionalArticles.Keys.ElementAt(i);
                List<(int articleID, int paragraphIndex, bool suppressed)> entry = page.info.AdditionalArticles[title];
                if (first)
                {
                    await writer.Append(HTMLTags.StartHeader +
                        "Additional References" +
                        HTMLTags.EndHeader);
                    first = false;
                }
                int id = page.info.AdditionalArticleIDs[title];
                if (id != lastID)
                {
                    await AppendArticleTitleLabel(writer, page, id, title, entry[Ordinals.first].suppressed);
                    lastID = id;
                }
                else
                {
                    await StartParagraph(writer, entry[Ordinals.first].suppressed);
                }
                for (int j = Ordinals.first; j < entry.Count; j++)
                {
                    parser.SetParagraphInfo(ParagraphType.Article, entry[j].articleID);
                    if (j > Ordinals.first)
                    {
                        await StartParagraph(writer, entry[j].suppressed);
                    }
                    string paragraph = await DataRepository.GlossaryParagraph(entry[j].articleID, entry[j].paragraphIndex);
                    await parser.ParseParagraph(paragraph, entry[j].paragraphIndex);
                }
            }
            (int start, int end) lastRange = (-1, -1);
            for (int i = Ordinals.first; i < page.info.AdditionalCrossReferences.Count; i++)
            {
                (int start, int end) range = page.info.AdditionalCrossReferences.Keys.ElementAt(i);
                List<(int articleID, int paragraphIndex, bool suppressed)> entry = page.info.AdditionalCrossReferences[range];
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
                    Citation labelCitation = new Citation(range.start, range.end);
                    labelCitation.CitationType = CitationTypes.Text;
                    labelCitation.Navigating = true;
                    await CitationConverter.AppendLink(writer, labelCitation, page.citation);
                    await writer.Append(": ");
                    lastRange = range;
                }
                else
                {
                    await StartParagraph(writer, entry[Ordinals.first].suppressed);
                }
                bool firstParagraph = true;
                for (int j = Ordinals.first; j < entry.Count; j++)
                {
                    (int articleID, int paragraphIndex, bool suppressed) = entry[j];
                    if (firstParagraph) firstParagraph = false;
                    else
                    {
                        await StartParagraph(writer, suppressed);
                    }
                    parser.SetParagraphInfo(ParagraphType.Comment, articleID);
                    string paragraph = await DataRepository.CommentParagraph(articleID, paragraphIndex);
                    if (paragraph == null) continue;
                    await parser.ParseParagraph(paragraph, paragraphIndex);
                }
            }
        }

        private static async Task StartRubySection(HTMLWriter writer, VersePage page)
        {
            await writer.Append(HTMLTags.StartSectionWithClass +
                HTMLClasses.scripture +
                HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartMainHeader);
            await page.WriteTitle(writer);
            await writer.Append(HTMLTags.EndMainHeader +
                HTMLTags.StartDivWithClass +
                HTMLClasses.textquote +
                Symbol.space +
                HTMLClasses.versetext +
                HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartParagraph
                );
        }

        internal static async Task StartParagraph(HTMLWriter writer, bool suppressed)
        {
            await writer.Append(HTMLTags.StartParagraphWithClass +
                HTMLClasses.comment);
            if (suppressed)
            {
                await writer.Append(Symbol.space + HTMLClasses.suppressed);
            }
            await writer.Append(HTMLTags.CloseQuoteEndTag);
        }
        private static async Task AppendArticleTitleLabel(HTMLWriter writer, VersePage page, int articleID, string title, bool suppressed)
        {
            await StartParagraph(writer, suppressed);
            await WriteTagAnchor(writer, title, articleID, page.citation);
            await writer.Append(": ");
        }

        private static async Task WriteOriginalWordLabel(HTMLWriter writer, int index, VersePageInfo info)
        {
            var phrase = info.Phrases[index];
            if (phrase.Weight == VersePageInfo.originalWordWeight)
            {
                await WriteOriginalWordLabel(writer, phrase, info);
                return;
            }
            List<(string text, (int start, int end) range)> originalWordsInPhrase = (from w in info.OriginalWords
                                                                                where w.Start <= phrase.Range.end &&
                                                                                w.Last >= phrase.Range.start
                                                                                select (w.Text, w.Range)).ToList();
            int count = originalWordsInPhrase.Count();
            if (count == 0) return;
            await writer.Append(HTMLTags.StartParagraphWithClass +
                HTMLClasses.comment +
                HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartBold);

            if (info.Ellipses.ContainsKey(phrase.Start))
                await WriteRangeText(writer, info, phrase);
            else
                await WriteRangeText(writer, phrase.Range, info);
            await writer.Append(HTMLTags.EndBold + "(" + HTMLTags.StartSpanWithClass +
                HTMLClasses.originalword +
                HTMLTags.CloseQuoteEndTag);
            bool needSpace = false;
            for (int i = Ordinals.first; i<count; i++)
            {
                if (info.Ellipses.ContainsKey(phrase.Start))
                {
                    if ((originalWordsInPhrase[i].range.start == info.Ellipses[phrase.Start].StartID.ID) &&
                        (originalWordsInPhrase[i].range.end == info.Ellipses[phrase.Start].EndID.ID))
                    {
                        continue;
                    }
                }
                if (needSpace) await writer.Append(Symbol.space);
                await writer.Append(originalWordsInPhrase[i].text.Replace('_', ' '));
                needSpace = true;
                i++;
            }
            await writer.Append(HTMLTags.EndSpan + ")");
        }

        private static async Task WriteOriginalWordLabel(HTMLWriter writer, VerseWord phrase, VersePageInfo info)
        {
            await writer.Append(HTMLTags.StartParagraphWithClass +
                           HTMLClasses.comment +
                           HTMLTags.CloseQuoteEndTag +
                           HTMLTags.StartBold);

            await WriteRangeText(writer, phrase.Range, info);
            await writer.Append(HTMLTags.EndBold + "(" + HTMLTags.StartSpanWithClass +
                HTMLClasses.originalword +
                HTMLTags.CloseQuoteEndTag);
            await writer.Append(phrase.Text.Replace('_', ' '));
            await writer.Append(HTMLTags.EndSpan + ")");
        }

        private static async Task WritePhraseComment(HTMLWriter writer, int index, VersePage page)
        {
            bool needFullLabel = true;
            int usedIndex = Result.notfound;
            if (page.info.OriginalWordComments.ContainsKey(index))
                needFullLabel = await WriteOriginalWordComments(writer, page, index);
            if (page.info.OriginalWordCrossReferences.ContainsKey(index))
                needFullLabel = await WriteOriginalWordCrossreferences(writer, page, index, needFullLabel);
            if (needFullLabel && page.info.PhraseArticles.ContainsKey(index))
                (needFullLabel, usedIndex) =
                        await WriteExactMatchPhraseArticle(writer, page, index, needFullLabel);
            await WritePhraseArticles(writer, page, index, needFullLabel, usedIndex);
        }

        internal static async Task WriteTagAnchor(HTMLWriter writer, string label, int articleID, Citation citation)
        {
            await PageFormatter.WriteTagAnchor(writer, label, articleID, citation);
        }
        private static async Task WritePhraseArticles(HTMLWriter writer, VersePage page, int index, bool needFullLabel, int usedIndex)
        {
            int currentIndex = Ordinals.first;
            if (page.info.PhraseArticles.ContainsKey(index))
            {
                PageParser parser = new PageParser(writer);
                parser.HideDetails();
                parser.SetTargetRange(page.citation);
                parser.SetStartHTML("");
                parser.SetEndHTML(HTMLTags.EndParagraph);
                for (int i = Ordinals.first; i < page.info.PhraseArticles[index].Count; i++)
                {
                    string title = page.info.PhraseArticles[index].Keys.ElementAt(i);
                    ((int start, int end) range,
                    int articleID, List<int> paragraphIndices) entry = page.info.PhraseArticles[index][title];
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
                        await WriteFullLabel(writer, page, index,
                            entry.range, entry.articleID, title);
                        needFullLabel = false;
                        needLabel = false;
                    }
                    parser.SetParagraphInfo(ParagraphType.Article, entry.articleID);
                    for (int j=Ordinals.first; j<entry.paragraphIndices.Count; j++)
                    {
                        if (j > Ordinals.first) 
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
                            await WriteTagAnchor(writer, title, entry.articleID, page.citation);
                            await writer.Append(": ");
                            needLabel = false;
                        }
                        string paragraph = await DataRepository.GlossaryParagraph(entry.articleID, entry.paragraphIndices[j]);
                        await parser.ParseParagraph(paragraph, entry.paragraphIndices[j]);
                    }
                    currentIndex++;
                }
            }
        }
        private static async Task<bool> WriteOriginalWordComments(HTMLWriter writer, VersePage page, int index)
        {
            PageParser parser = new PageParser(writer);
            parser.HideDetails();
            parser.SetTargetRange(page.citation);
            bool needFullLabel = true;
            await WriteFullOriginalWordLabel(writer, page.info, index);
            await writer.Append(": ");
            for (int i = Ordinals.first; i < page.info.OriginalWordComments.Count; i++)
            {
                parser.SetParagraphInfo(ParagraphType.Comment, page.info.OriginalWordComments[index]);
                List<string> paragraphs = TextSectionFormatter.ReadParagraphs(
                    page.info.OriginalWordComments[index]);
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

        private static async Task<bool> WriteOriginalWordCrossreferences(HTMLWriter writer, VersePage page, int index, bool needFullLabel)
        {
            PageParser parser = new PageParser(writer);
            parser.SetTargetRange(page.citation);
            parser.HideDetails();
            for (int i = Ordinals.first; i < page.info.OriginalWordCrossReferences[index].Count; i++)
            {
                int articleID = page.info.OriginalWordCrossReferences[index][i].articleID;
                int paragraphIndex = page.info.OriginalWordCrossReferences[index][i].paragraphIndex;
                if (needFullLabel)
                {
                    await WriteFullCrossReferenceLabel(writer, page.info, index, articleID);
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
                string paragraph = await DataRepository.CommentParagraph(articleID, paragraphIndex);
                await parser.ParseParagraph(paragraph, paragraphIndex);
            }
            return needFullLabel;
        }

        private static async Task WriteFullCrossReferenceLabel(HTMLWriter writer, VersePageInfo info, int index, int commentID)
        {
            (int start, int end) phraseRange = info.Phrases[index].Range;

            var originalWordsInPhrase = (from w in info.OriginalWords
                                         where w.Start >= phraseRange.start &&
                                         w.Last <= phraseRange.end
                                         select w.Text).ToList();
            int count = originalWordsInPhrase.Count();
            await writer.Append(HTMLTags.StartParagraphWithClass +
                HTMLClasses.comment +
                HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartBold);

            await WriteRangeText(writer, info.Phrases[index].Range, info);
            await writer.Append(HTMLTags.EndBold + "(");

            await WriteVerseLabel(writer, commentID);
            if (count > 0) await writer.Append("; " + HTMLTags.StartSpanWithClass +
                HTMLClasses.originalword +
                HTMLTags.CloseQuoteEndTag);

            for (int i = Ordinals.first; i < originalWordsInPhrase.Count; i++)
            {
                if (i > Ordinals.first) await writer.Append(Symbol.space);
                await writer.Append(originalWordsInPhrase[i].Replace('_', ' '));
            }
            if (count > 0) await writer.Append(HTMLTags.EndSpan);
            await writer.Append(")");
        }
        private static async Task WriteVerseLabel(HTMLWriter writer, int commentID)
        {
            (int start, int end) range = (await DataRepository.CommentLinks(commentID)).First();
            Citation crossreference = new Citation(range.start, range.end);
            crossreference.CitationType = (crossreference.Length < 10) ?
                 CitationTypes.Verse :
                 CitationTypes.Text;
            crossreference.Navigating = true;
            await CitationConverter.AppendLink(writer, crossreference);
        }

        private static async Task WriteFullOriginalWordLabel(HTMLWriter writer, VersePageInfo info, int index)
        {
            (int start, int end) phraseRange = info.Phrases[index].Range;

            List<(string text, (int start, int end) range)> originalWordsInPhrase = (from w in info.OriginalWords
                                                                                where w.Start >= phraseRange.start &&
                                                                                w.Last <= phraseRange.end
                                                                                select (w.Text, w.Range)).ToList();
            int count = originalWordsInPhrase.Count();
            await writer.Append(HTMLTags.StartParagraphWithClass +
                HTMLClasses.comment +
                HTMLTags.CloseQuoteEndTag +
                HTMLTags.StartBold);
            var phrase = info.Phrases[index];
            if (info.Ellipses.ContainsKey(phrase.Start))
                await WriteRangeText(writer, info, phrase);
            else
                await WriteRangeText(writer, info.Phrases[index].Range, info);
            await writer.Append(HTMLTags.EndBold);
            if (count > 0) await writer.Append("(" + HTMLTags.StartSpanWithClass +
                HTMLClasses.originalword +
                HTMLTags.CloseQuoteEndTag);
            for (int i=Ordinals.first; i<originalWordsInPhrase.Count; i++)
            {
                if (info.Ellipses.ContainsKey(phrase.Start))
                {
                    if ((originalWordsInPhrase[i].range.start == info.Ellipses[phrase.Start].StartID.ID) &&
                        (originalWordsInPhrase[i].range.end == info.Ellipses[phrase.Start].EndID.ID))
                    {
                        continue;
                    }
                }
                if (i>Ordinals.first) await writer.Append(Symbol.space);
                await writer.Append(originalWordsInPhrase[i].text.Replace("_", HTMLTags.NonbreakingSpace));
            }
            if (count > 0) await writer.Append(HTMLTags.EndSpan + ")");
        }
        private static async Task<(bool needFullLabel, int usedIndex)>
            WriteExactMatchPhraseArticle(HTMLWriter writer, VersePage page, int index, bool needFullLabel)
        {
            (int start, int end) range = page.info.PhraseArticles[index].First().Value.range;
            if ((range.end - range.start) > 8) return (needFullLabel, Result.notfound);
            int resultIndex = Result.notfound;
            string offsetLabel = ReadRangeText(range, page.info).Trim();
            List<string> offsetRoots = await Inflections.RootsOf(offsetLabel);
            string offsetRoot = ((offsetRoots.Count > Number.nothing) && (!string.IsNullOrEmpty(offsetRoots[Ordinals.first]))) ?
                offsetRoots[Ordinals.first] :
                offsetLabel;
            PageParser parser = new PageParser(writer);
            parser.SetTargetRange(page.citation);
            parser.HideDetails();
            parser.SetStartHTML("");
            parser.SetEndHTML(HTMLTags.EndParagraph);
            for (int usedIndex = Ordinals.first; usedIndex < page.info.PhraseArticles[index].Count; usedIndex++)
            {
                string title = page.info.PhraseArticles[index].Keys.ElementAt(usedIndex);
                ((int start, int end) range,
                int articleID, List<int> paragraphIndices) entry = page.info.PhraseArticles[index][title];
                if ((title == offsetRoot ||
                    (title == offsetLabel)))
                {
                    await writer.Append(HTMLTags.StartParagraphWithClass +
                        HTMLClasses.comment +
                        HTMLTags.CloseQuoteEndTag);
                    await WriteFullLabel(writer, page, index, entry.range, entry.articleID, title);

                    parser.SetParagraphInfo(ParagraphType.Article, entry.articleID);
                    for (int i=Ordinals.first; i<entry.paragraphIndices.Count; i++)
                    {
                        if (i > Ordinals.first) 
                        {
                            await writer.Append(HTMLTags.StartParagraphWithClass +
                                HTMLClasses.comment +
                                HTMLTags.CloseQuoteEndTag);
                        }
                        string paragraph = await DataRepository.GlossaryParagraph(entry.articleID, entry.paragraphIndices[i]);
                        await parser.ParseParagraph(paragraph, entry.paragraphIndices[i]);
                    }
                    needFullLabel = false;
                    resultIndex = usedIndex;
                    break;
                }
            }

            if (resultIndex == Result.notfound)
            {
                for (int usedIndex = Ordinals.first; usedIndex < page.info.PhraseArticles[index].Count; usedIndex++)
                {
                    string title = page.info.PhraseArticles[index].Keys.ElementAt(usedIndex);
                    ((int start, int end) range,
                    int articleID, List<int> paragraphIndices) entry = page.info.PhraseArticles[index][title];
                    List<string> synonyms = await DataRepository.Synonyms(entry.articleID);
                    if (synonyms.Contains(offsetRoot) ||
                        synonyms.Contains(await AllWords.Conform(offsetLabel)))
                    {
                        await writer.Append(HTMLTags.StartParagraphWithClass +
                            HTMLClasses.comment +
                            HTMLTags.CloseQuoteEndTag);
                        await WriteFullLabel(writer, page, index, entry.range, entry.articleID, title);
                        parser.SetParagraphInfo(ParagraphType.Article, entry.articleID);
                        for (int i=Ordinals.first; i<entry.paragraphIndices.Count; i++)
                        {
                            if (i>Ordinals.first)
                            {
                                await writer.Append(HTMLTags.StartParagraphWithClass +
                                    HTMLClasses.comment +
                                    HTMLTags.CloseQuoteEndTag);
                            }
                            string paragraph = await DataRepository.GlossaryParagraph(entry.articleID, entry.paragraphIndices[i]);
                            await parser.ParseParagraph(paragraph, entry.paragraphIndices[i]);
                        }
                        needFullLabel = false;
                        resultIndex = usedIndex;
                        break;
                    }
                }
            }
            return (needFullLabel, resultIndex);
        }

        private static async Task WriteFullLabel(HTMLWriter writer, VersePage page, int index, (int start, int end) wordRange, int articleID, string articleTitle)
        {
            (int start, int end) phraseRange = page.info.Phrases[index].Range;
            string label = ReadRangeText(phraseRange, page.info);
            label = label.Trim();
            List<string> synonyms = await DataRepository.Synonyms(articleID);
            List<string> roots = await Inflections.RootsOf(label);
            string root = ((roots.Count > Number.nothing) && (!string.IsNullOrEmpty(roots[Ordinals.first]))) ?
                roots[Ordinals.first] :
                label;
            string searchRoot = await SearchPhrase(phraseRange);
            bool substitute = !(synonyms.Contains(root) || synonyms.Contains(searchRoot) ||
                synonyms.Contains(await AllWords.Conform(label)));
            bool part = false;

            if (substitute)
            {
                if (!wordRange.Equals(phraseRange))
                {
                    string offsetLabel = (await SearchPhrase(wordRange)).Replace('_', ' ').TrimEnd();
                    List<string> offsetRoots = await Inflections.RootsOf(offsetLabel);
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
                        await VerseFormatter.WriteTagAnchor(writer, offsetLabel, articleID, page.citation);
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
                await VerseFormatter.WriteTagAnchor(writer, label, articleID, page.citation);
            }
            label = page.info.OriginalWordsInRange(phraseRange.start, phraseRange.end);
            if (string.IsNullOrEmpty(label))
            {
                if (substitute)
                {
                    string substituteWord = await DataRepository.SubstituteWord(phraseRange.start);
                    if (synonyms.Contains(substituteWord)) articleTitle = substituteWord;
                    await writer.Append(HTMLTags.EndBold + " (");
                    await VerseFormatter.WriteTagAnchor(writer, articleTitle, articleID, page.citation);
                    await writer.Append("): ");
                    return;
                }
                await writer.Append(":" + HTMLTags.EndBold + Symbol.space);
                return;
            }
            if (substitute)
            {
                string cleanLabel = Inflections.RemoveDiacritics(label);
                if (synonyms.Contains(cleanLabel))
                {
                    await writer.Append(HTMLTags.EndBold +
                        " (" + HTMLTags.StartSpanWithClass +
                        HTMLClasses.originalword +
                        HTMLTags.CloseQuoteEndTag);
                    await VerseFormatter.WriteTagAnchor(writer, label, articleID, page.citation);
                    await writer.Append(HTMLTags.EndSpan + "): ");
                    return;
                }
                string substituteWord = await DataRepository.SubstituteWord(phraseRange.start);
                if (synonyms.Contains(substituteWord)) articleTitle = substituteWord;
                await writer.Append(HTMLTags.EndBold + " (");
                await VerseFormatter.WriteTagAnchor(writer,
                    articleTitle.Replace('(', '[').Replace(')', ']'),
                    articleID, page.citation);
                await writer.Append("; " + HTMLTags.StartSpanWithClass +
                    HTMLClasses.originalword +
                    HTMLTags.CloseQuoteEndTag);
                await writer.Append(label);
                await writer.Append(HTMLTags.EndSpan + "): ");
                return;
            }
            await writer.Append(HTMLTags.EndBold +
                " (" + HTMLTags.StartSpanWithClass +
                HTMLClasses.originalword +
                HTMLTags.CloseQuoteEndTag);
            await writer.Append(label);
            await writer.Append(HTMLTags.EndSpan + "): ");
        }
        internal static string ReadRangeText((int start, int end) range, VersePageInfo info)
        {
            StringBuilder result = new StringBuilder();
            var wordsInRange = (from w in info.keywords
                               where w.ID >= range.start &&
                               w.ID <= range.end
                               select w).ToList();
            bool opened = false;
            for (int i= Ordinals.first; i<wordsInRange.Count; i++)
            {
                if ((i>Ordinals.first) && (HasOpenBracket(wordsInRange[i]))) opened = true;
                if (!opened) result.Append(TextWithoutSymbols(wordsInRange[i]));
                else
                    result.Append(FormattedTextWithBrackets(wordsInRange[i]));
                if (HasCloseBracket(wordsInRange[i])) opened = false;
            }
            if (opened) result.Append(']');
            return result.ToString();
        }

        private static bool HasOpenBracket(Keyword word)
        {
            return word.LeadingSymbols.Contains('[');
        }
        private static bool HasCloseBracket(Keyword word)
        {
            return word.TrailingSymbols.Contains(']');
        }
        internal static string TextWithoutSymbols(Keyword word)
        {
            StringBuilder result = new StringBuilder();
            result.Append(word.CapitalizedText.Replace('`', '’'));
            result.Append(' ');
            result.Replace("<br>", "");
            return result.ToString();
        }

        internal static string FormattedTextWithBrackets(Keyword word)
        {
            StringBuilder result = new StringBuilder();
            if (word.LeadingSymbols.IndexOf('[') != Result.notfound) result.Append('[');
            result.Append(word.CapitalizedText.Replace('`', '’'));
            if (word.TrailingSymbols.IndexOf(']') != Result.notfound) result.Append(']');
            result.Append(' ');
            result.Replace("<br>", "");
            return result.ToString();
        }
        private static async Task WriteRangeText(HTMLWriter writer, 
            VersePageInfo info, 
            VerseWord phrase)
        {
            var wordsInRange = (from w in info.keywords
                               where w.ID >= phrase.Range.start &&
                               w.ID <= phrase.Range.end
                               select w).ToList();
            bool opened = false;
            bool ellipsis = false;
            for (int i=Ordinals.first; i<wordsInRange.Count; i++)
            {
                if ((wordsInRange[i].ID >= info.Ellipses[phrase.Start].Range.start) && 
                    (wordsInRange[i].ID <= info.Ellipses[phrase.Start].Range.end))
                {
                    if (!ellipsis) await writer.Append("... ");
                    ellipsis = true;
                    continue;
                }
                ellipsis = false;
                if ((i>Ordinals.first) && (HasOpenBracket(wordsInRange[i]))) opened = true;
                if (!opened) await writer.Append(TextWithoutSymbols(wordsInRange[i]));
                else
                    await writer.Append(FormattedTextWithBrackets(wordsInRange[i]));
                if (HasCloseBracket(wordsInRange[i])) opened = false;
            }
            if (opened) await writer.Append(']');
        }
        private static async Task WriteRangeText(HTMLWriter writer, (int start, int end) range, VersePageInfo info)
        {
            var wordsInRange = (from w in info.keywords
                               where w.ID >= range.start &&
                               w.ID <= range.end
                               select w).ToList();

            bool opened = false;
            for (int i = Ordinals.first; i < wordsInRange.Count; i++)
            {
                if ((i>Ordinals.first) && (HasOpenBracket(wordsInRange[i]))) opened = true;
                if (!opened) await writer.Append(TextWithoutSymbols(wordsInRange[i]));
                else
                    await writer.Append(FormattedTextWithBrackets(wordsInRange[i]));
                if (HasCloseBracket(wordsInRange[i])) opened = false;
            }
            if (opened) await writer.Append(']');
        }

        private static async Task<string> SearchPhrase((int start, int end) wordRange)
        {
            List<string> words = await DataRepository.SearchPhrase(wordRange.start, wordRange.end);
            StringBuilder result = new StringBuilder();
            for (int i = Ordinals.first; i < words.Count; i++)
            {
                if (i > Ordinals.first) result.Append(' ');
                result.Append(words[i]);
            }
            return result.ToString();
        }

    }
}
