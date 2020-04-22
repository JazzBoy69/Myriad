using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feliciana.Library;
using Feliciana.Data;
using Myriad.Data;
using Myriad.Library;
using Myriad.Formatter;

namespace Myriad.Pages
{
    public class VersePageInfo
    {
        public const ushort originalWordWeight = 200;
        List<VerseWord> words;
        List<RangeAndParagraph> relatedArticles;
        List<(int commentID, int paragraphIndex)> usedReferences = new List<(int articleID, int paragraphIndex)>();
        readonly List<(int articleID, int paragraphIndex)> usedArticles = new List<(int articleID, int paragraphIndex)>();

        public List<VerseWord> Phrases { get; } = new List<VerseWord>();
        public IEnumerable<VerseWord> OriginalWords { get; private set; }
        public Dictionary<int, CitationRange> Ellipses { get; } = new Dictionary<int, CitationRange>();
        public Dictionary<int, Dictionary<string, ((int start, int end) range, int articleID, 
            List<int> paragraphIndices)>> PhraseArticles { get; } = 
            new Dictionary<int, Dictionary<string, ((int start, int end) range, 
                int articleID, List<int> paragraphIndices)>>();
        public Dictionary<string, int> AdditionalArticleIDs { get; } = new Dictionary<string, int>();
        public SortedDictionary<(int startID, int endID), 
            List<(int articleID, int paragraphIndex, bool suppressed)>> 
            AdditionalCrossReferences { get; } = 
            new SortedDictionary<(int startID, int endID), 
                List<(int articleID, int paragraphIndex, bool suppressed)>>();
        public VersePageInfo()
        {

        }

        public Dictionary<int, List<(int articleID, int paragraphIndex)>> OriginalWordCrossReferences { get; } = new Dictionary<int, List<(int articleID, int paragraphIndex)>>();
        public Dictionary<int, List<(int articleID, int paragraphIndex)>> OriginalWordComments { get; } = new Dictionary<int, List<(int articleID, int paragraphIndex)>>();
        public SortedDictionary<string, List<(int articleID, int paragraphIndex, bool suppressed)>> AdditionalArticles { get; } = new SortedDictionary<string, List<(int articleID, int paragraphIndex, bool suppressed)>>();
        public async Task LoadInfo(CitationRange citationRange)
        {
            ReadSearchWords(citationRange);
            FindPhrases(citationRange);
            await ArrangeRelatedArticles(citationRange);
            await ArrangeDefinitionSearches(citationRange);
            await ArrangeCrossReferences(citationRange);
            ArrangeOriginalWordComments(citationRange);
            await ArrangeAdditionalRelatedArticles(citationRange);
        }

        private async Task ArrangeCrossReferences(CitationRange citationRange)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadVerseCrossReferences),
                citationRange.StartID.ID, citationRange.EndID.ID);
            List<RangeAndParagraph> crossreferences = reader.GetClassData<RangeAndParagraph>();
            reader.Close();
            for (int index = Ordinals.first; index<crossreferences.Count; index++)
            {
                var reference = crossreferences[index];
                if (usedReferences.Contains(reference.Key))
                {
                    continue;
                }
                usedReferences.Add(reference.Key);
                if ((reference.StartID < citationRange.StartID.ID) ||
                    (reference.EndID > citationRange.EndID.ID))
                //reference starts at a preceeding verse
                {
                    List<(int start, int end)> links = await TextSectionFormatter.ReadLinks(reference.ArticleID);
                    if (reference.StartID < citationRange.StartID.ID)
                    {
                        // add to See Also
                        if (links.Count > 0)
                        {
                            if (AdditionalCrossReferences.ContainsKey(links.First()))
                                AdditionalCrossReferences[links.First()].Add((reference.ArticleID,
                                    reference.ParagraphIndex, true));
                            else
                                AdditionalCrossReferences.Add(links.First(),
                                    new List<(int articleID, int paragraphIndex, bool suppressed)>() {
                                            (reference.ArticleID, reference.ParagraphIndex, true)});
                        }
                    }
                    else
                    {
                        if (links.Count > 0)
                        {
                            if (AdditionalCrossReferences.ContainsKey(links.First()))
                                AdditionalCrossReferences[links.First()].Add((reference.ArticleID,
                                    reference.ParagraphIndex, false));
                            else
                                AdditionalCrossReferences.Add(links.First(),
                                    new List<(int articleID, int paragraphIndex, bool suppressed)>() {
                                            (reference.ArticleID, reference.ParagraphIndex, false)});
                        }
                    }
                    continue;
                }
                int bottom = Ordinals.first;
                int top = Phrases.Count - 1;
                if ((reference.StartID == citationRange.StartID.ID) &&
                    (reference.EndID == citationRange.EndID.ID))
                {
                    bottom = -1;
                    top = -1;
                }

                int phraseIndex = -1;
                while (bottom != top)
                {
                    int mid = (top - bottom) / 2 + bottom;
                    if (Phrases[mid].End < reference.StartID)
                    {
                        if (bottom == mid) bottom++; else bottom = mid;
                        continue;
                    }
                    if (Phrases[mid].Start > reference.EndID)
                    {
                        if (top == mid) top--; else top = mid;
                        continue;
                    }
                    if ((reference.StartID >= Phrases[mid].Start) &&
                        (reference.EndID <= Phrases[mid].End))
                    {
                        phraseIndex = mid;
                        AddOriginalWordCrossReference(reference, mid);
                    }
                    break;
                }
                if (((phraseIndex == -1) && (bottom < Phrases.Count)) &&
                    (((reference.StartID >= Phrases[bottom].Start) &&
                        (reference.EndID <= Phrases[bottom].End))))
                {
                    AddOriginalWordCrossReference(reference, bottom);
                    phraseIndex = bottom;
                }
                if (phraseIndex == -1)
                {
                    //if suppressing and not first verse in cross reference add to "See also"
                    List<(int start, int end)> links = await TextSectionFormatter.ReadLinks(reference.ArticleID);
                    if (links.Count > 0)
                    {
                        AddToAdditionalCrossReferences(reference, links.First(),
                            (reference.StartID < citationRange.StartID.ID));
                    }
                }
            }
        }

        private void ArrangeOriginalWordComments(CitationRange citationRange)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadLinkedParagraphs),
                citationRange.StartID.ID, citationRange.EndID.ID); 
            List<RangeAndParagraph> linkedParagraphs = reader.GetClassData<RangeAndParagraph>();
            reader.Close();
            for (int index = Ordinals.first; index<linkedParagraphs.Count; index++)
            {
                var reference = linkedParagraphs[index];
                if (usedReferences.Contains(reference.Key))
                {
                    continue;
                }
                usedReferences.Add(reference.Key);
                if (OriginalWordsInRange(reference.StartID, reference.EndID).Length == Number.nothing) continue;
                if ((reference.StartID < citationRange.StartID.ID) ||
                    (reference.EndID > citationRange.EndID.ID))
                {
                    continue;
                }
                int bottom = Ordinals.first;
                int top = Phrases.Count - 1;
                int phraseIndex = -1;
                while (bottom != top)
                {
                    int mid = (top - bottom) / 2 + bottom;
                    if (Phrases[mid].End < reference.StartID)
                    {
                        if (bottom == mid) bottom++; else bottom = mid;
                        continue;
                    }
                    if (Phrases[mid].Start > reference.EndID)
                    {
                        if (top == mid) top--; else top = mid;
                        continue;
                    }
                    if ((reference.StartID >= Phrases[mid].Start) &&
                        (reference.EndID <= Phrases[mid].End))
                    {
                        phraseIndex = mid;
                        AddOriginalWordComment(reference, mid);
                        usedReferences.Add(reference.Key);
                    }
                    break;
                }
                if (((phraseIndex == -1) && (bottom < Phrases.Count)) &&
                    (((reference.StartID >= Phrases[bottom].Start) &&
                        (reference.EndID <= Phrases[bottom].End))))
                {
                    AddOriginalWordComment(reference, bottom);
                    phraseIndex = bottom;
                }
            }
        }

        private void AddOriginalWordComment(RangeAndParagraph rangeAndParagraph, int index)
        {
            if (OriginalWordComments.ContainsKey(index))
            {
                OriginalWordComments[index].Add(rangeAndParagraph.Key);
                return;
            }
            OriginalWordComments.Add(index, new List<(int articleID, int paragraphIndex)>()
                { rangeAndParagraph.Key });
        }

        public string OriginalWordsInRange(int start, int end)
        {
            IEnumerable<string> originalWordsInPhrase = from w in OriginalWords
                                                        where w.Start >= start &&
                                                        w.End <= end
                                                        select w.Text;
            int count = originalWordsInPhrase.Count();
            if (count == 0) return "";
            int i = Ordinals.first;
            StringBuilder sb = new StringBuilder();
            foreach (string word in originalWordsInPhrase)
            {
                sb.Append(word.Replace('_', ' '));
                if (i < count - 1) sb.Append(" ");
                i++;
            }
            return sb.ToString();
        }

        private void AddOriginalWordCrossReference(RangeAndParagraph rangeAndParagraph, int index)
        {
            if (OriginalWordCrossReferences.ContainsKey(index))
                OriginalWordCrossReferences[index].Add(rangeAndParagraph.Key);
            else
                OriginalWordCrossReferences.Add(index, new List<(int articleID, int paragraphIndex)>() {
                    rangeAndParagraph.Key
                });
        }
        private void AddToAdditionalCrossReferences(RangeAndParagraph reference, (int start, int end) range, bool show)
        {
            if (AdditionalCrossReferences.ContainsKey(range))
                AdditionalCrossReferences[range].Add(
                    (reference.ArticleID, reference.ParagraphIndex, show));
            else
                AdditionalCrossReferences.Add(range,
                    new List<(int articleID, int paragraphIndex, bool suppressed)>() {
                                    (reference.ArticleID, reference.ParagraphIndex, false)});
        }

        private void ReadSearchWords(CitationRange citationRange)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadVerseWords),
                citationRange.StartID.ID, citationRange.EndID.ID);
            words = reader.GetClassData<VerseWord>();
            reader.Close();
        }

        private void FindPhrases(CitationRange citationRange)
        {
            OriginalWords = from w in words
                            where w.Weight == originalWordWeight
                            select w;
            int start = citationRange.StartID.ID;

            var lastPhrase = new VerseWord();
            for (int index = Ordinals.first; index < words.Count; index++)
            {
                var word = words[index];

                if ((lastPhrase.Weight == originalWordWeight) && 
                    (word.Start < lastPhrase.End) && 
                    (word.Start > lastPhrase.Start) &&
                    (word.Weight == originalWordWeight))
                {
                    if (Ellipses.ContainsKey(lastPhrase.Start))
                    {
                        Ellipses[lastPhrase.Start].ExtendTo(word.End);
                    }
                    else
                        Ellipses.Add(lastPhrase.Start, new CitationRange(word.Start, word.End));
                    Phrases.Add(word);
                    continue;
                }
                if (word.Start < start)
                {
                    continue;
                }
                Phrases.Add(word);
                if (word.End - word.Start > 1)
                {
                    var owords = from w in OriginalWords
                                 where w.Start == word.Start &&
                                 w.End == word.End
                                 select w;
                    if (owords.Any())
                        lastPhrase = owords.First();
                }
                start = word.End + 1;
            }
        }
        private async Task ArrangeDefinitionSearches(CitationRange citationRange)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadDefinitionSearchesInVerse),
                citationRange.StartID.ID, citationRange.EndID.ID);
            List<(int, int, int)> definitionsearches = await reader.GetData<int, int, int>();
            reader.Close();
            var used = new List<(int start, int end, int id)>();
            for (int index = Ordinals.first; index < definitionsearches.Count; index++) 
            {
                if (!used.Contains(definitionsearches[index]))
                {
                    used.Add(definitionsearches[index]);
                    //definitionSearches.Add((new Range(item.start, item.end), item.id));
                }
                await ArrangeDefinitionSearch(definitionsearches[index]);
            }
        }


        private async Task ArrangeRelatedArticles(CitationRange citationRange)
        {
            var reader = new DataReaderProvider<int, int>(SqlServerInfo.GetCommand(DataOperation.ReadRelatedArticles),
                citationRange.StartID.ID, citationRange.EndID.ID);
            relatedArticles = reader.GetClassData<RangeAndParagraph>();

            foreach (var article in relatedArticles)
            {
                if (usedArticles.Contains(article.Key))
                {
                    continue;
                }
                if (((article.EndID-article.StartID)<10) && (article.StartID >= citationRange.StartID.ID) && 
                    (article.EndID <= citationRange.EndID.ID) &&
                        ((article.StartID != citationRange.StartID.ID) || (article.EndID != citationRange.EndID.ID)))
                { // Add an article reference to a single phrase in verse to definition searches
                    await ArrangeDefinitionSearch((article.StartID, article.EndID, article.ArticleID));
                    continue;
                }
            }
        }

        private async Task ArrangeDefinitionSearch((int start, int end, int id) item)
        {
            int bottom = Ordinals.first;
            int top = Phrases.Count - 1;
            bool found = false;
            while (bottom <= top)
            {
                int mid = (top - bottom) / 2 + bottom;
                if (Phrases[mid].End < item.start)
                {
                    if (bottom == mid) bottom++; else bottom = mid;
                    continue;
                }
                if (Phrases[mid].Start > item.end)
                {
                    if (top == mid) top--; else top = mid;
                    continue;
                }
                if ((Ellipses.ContainsKey(Phrases[mid].Start)) &&
                    (Ellipses[Phrases[mid].Start].Contains(new CitationRange(item.start, item.end))))
                {
                    if (bottom == mid) bottom++; else bottom = mid;
                    continue;
                }
                found = true;
                await AddComment(((item.start, item.end), item.id), mid);
                break;
            }
            if ((!found) && (bottom < Phrases.Count))
            {
                await AddComment(((item.start, item.end), item.id), bottom);
                found = true;
            }
        }


        private async Task AddComment(((int start, int end) range, int articleID) commentInfo, int index)
        {
            //Add related article
            List<(int articleID, int paragraphIndex)> result = new List<(int articleID, int paragraphIndex)>();
            IEnumerable<int> articleParagraphs = (from paragraph in relatedArticles
                                                  where paragraph.ArticleID == commentInfo.articleID
                                                  orderby paragraph.ParagraphIndex
                                                  select paragraph.ParagraphIndex).Distinct();
            List<int> newParagraphs = new List<int>();
            foreach (int paragraphIndex in articleParagraphs)
            {
                if (usedArticles.Contains((commentInfo.articleID, paragraphIndex))) continue;
                newParagraphs.Add(paragraphIndex);
                usedArticles.Add((commentInfo.articleID, paragraphIndex));
            }
            if (newParagraphs.Count == 0) return;
            if (PhraseArticles.ContainsKey(index))
            {
                string articleTitle = await ArticleTitle(commentInfo.articleID);
                if (!PhraseArticles[index].ContainsKey(articleTitle))
                {
                    PhraseArticles[index].Add(articleTitle,
                        (commentInfo.range, commentInfo.articleID, newParagraphs));
                }
            }
            else
            {
                PhraseArticles.Add(index, new Dictionary<string, ((int start, int end) range, int articleID, List<int> paragraphIndices)>());
                PhraseArticles[index].Add(await ArticleTitle(commentInfo.articleID),
                    (commentInfo.range, commentInfo.articleID, newParagraphs));
            }
        }

        private async Task ArrangeAdditionalRelatedArticles(CitationRange citationRange)
        {
            //Loop through Related Articles to add unused articles to Additional References or "See also"
            int index = Ordinals.first;
            while (index < relatedArticles.Count)
            {
                if (usedArticles.Contains(relatedArticles[index].Key))
                {
                    index++;
                    continue;
                }
                usedArticles.Add(relatedArticles[index].Key);

                if (relatedArticles[index].StartID < citationRange.StartID.ID)
                {
                    await AddToAdditionalArticles(index, true);
                }
                else
                {
                    await AddToAdditionalArticles(index, await DefinitionSearchExistsInRange(relatedArticles[index].Range,
                       relatedArticles[index].ArticleID));
                }
                index++;
            }
        }

        private async Task<bool> DefinitionSearchExistsInRange((int start, int end) range, int articleID)
        {
            var reader = new DataReaderProvider<int, int, int>(SqlServerInfo.GetCommand(DataOperation.DefinitionSearchesInRange),
                range.start, range.end, articleID);
            List<(int start, int end)> ranges = await reader.GetData<int, int>();
            return ranges.Count > Number.nothing;
        }

        private async Task AddToAdditionalArticles(int index, bool show)
        {
            string title = await ArticleTitle(relatedArticles[index].ArticleID);
            if (AdditionalArticleIDs.ContainsKey(title))
                AdditionalArticles[title].Add((relatedArticles[index].ArticleID, relatedArticles[index].ParagraphIndex,
                    show));
            else
            {
                AdditionalArticles.Add(title, new List<(int articleID, int paragraphIndex, bool suppressed)>() {
                            (relatedArticles[index].ArticleID, relatedArticles[index].ParagraphIndex, show)});
                AdditionalArticleIDs.Add(title, relatedArticles[index].ArticleID);
            }
        }

        private static async Task<string> ArticleTitle(int articleID)
        {
            string title = await ArticlePage.ReadTitle(articleID);
            return title.Replace(' ', '_').Replace('\u0a00', '_');
        }

        public bool PhraseHasNoComments(int index)
        {
            return (!OriginalWordComments.ContainsKey(index) && 
                !PhraseArticles.ContainsKey(index) && 
                !OriginalWordCrossReferences.ContainsKey(index));
        }
    }
}
